using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using TaskManagement.API.Data;

var provider = GetArgument(args, "--provider")
    ?? throw new ArgumentException("--provider PostgreSql veya --provider Oracle gereklidir.");

if (provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
{
    await TestPostgreSqlAsync();
}
else if (provider.Equals("Oracle", StringComparison.OrdinalIgnoreCase))
{
    await TestOracleAsync();
}
else
{
    throw new ArgumentException("Provider PostgreSql veya Oracle olmalıdır.");
}

async Task TestPostgreSqlAsync()
{
    var adminConnectionString = Environment.GetEnvironmentVariable("PROVIDER_TEST_POSTGRES_ADMIN")
        ?? throw new InvalidOperationException("PROVIDER_TEST_POSTGRES_ADMIN tanımlı değil.");
    var databaseName = $"tms_provider_{Guid.NewGuid():N}"[..29];

    EnsureIdentifier(databaseName, "^tms_provider_[a-f0-9]+$");

    await using (var connection = new NpgsqlConnection(adminConnectionString))
    {
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            $"CREATE DATABASE \"{databaseName}\"",
            connection);
        await command.ExecuteNonQueryAsync();
    }

    var builder = new NpgsqlConnectionStringBuilder(adminConnectionString)
    {
        Database = databaseName,
        Pooling = false
    };

    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseNpgsql(builder.ConnectionString, providerOptions => providerOptions
            .MigrationsAssembly("TaskManagement.API.Migrations.PostgreSql")
            .MigrationsHistoryTable("__EFMigrationsHistory"))
        .Options;

    await ApplyAndVerifyMigrationsAsync(
        options,
        [
            "20260707201103_InitialCreate",
            "20260707210236_SeedDemoUser",
            "20260716133403_AddTaskAttachments",
            "20260716173653_AddTaskComments",
            "20260717153937_AddIndexes",
            "20260728110100_AddUserRole"
        ]);

    await VerifyApiAsync("PostgreSql", builder.ConnectionString);

    Console.WriteLine($"PASS: PostgreSQL migration/API testi tamamlandı. Korunan test DB: {databaseName}");
}

async Task TestOracleAsync()
{
    var adminConnectionString = Environment.GetEnvironmentVariable("PROVIDER_TEST_ORACLE_ADMIN")
        ?? throw new InvalidOperationException("PROVIDER_TEST_ORACLE_ADMIN tanımlı değil.");
    var useExistingSchema = args.Any(argument =>
        argument.Equals("--use-existing", StringComparison.OrdinalIgnoreCase));
    var schemaName = $"TMS_{Guid.NewGuid():N}"[..20].ToUpperInvariant();
    var schemaPassword = $"Aa1{Guid.NewGuid():N}";

    string schemaConnectionString;

    if (useExistingSchema)
    {
        schemaConnectionString = adminConnectionString;
    }
    else
    {
        EnsureIdentifier(schemaName, "^TMS_[A-F0-9]+$");

        await using (var connection = new OracleConnection(adminConnectionString))
        {
            await connection.OpenAsync();
            await ExecuteOracleAsync(
                connection,
                $"CREATE USER {schemaName} IDENTIFIED BY \"{schemaPassword}\"");
            await ExecuteOracleAsync(
                connection,
                $"GRANT CREATE SESSION, CREATE TABLE, CREATE SEQUENCE, CREATE TRIGGER, CREATE PROCEDURE TO {schemaName}");
            await ExecuteOracleAsync(
                connection,
                $"ALTER USER {schemaName} QUOTA UNLIMITED ON USERS");
        }

        var adminBuilder = new OracleConnectionStringBuilder(adminConnectionString);
        var schemaBuilder = new OracleConnectionStringBuilder
        {
            UserID = schemaName,
            Password = schemaPassword,
            DataSource = adminBuilder.DataSource,
            Pooling = false
        };
        schemaConnectionString = schemaBuilder.ConnectionString;
    }

    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseOracle(schemaConnectionString, providerOptions => providerOptions
            .MigrationsAssembly("TaskManagement.API.Migrations.Oracle")
            .MigrationsHistoryTable("__EFMigrationsHistory"))
        .Options;

    await ApplyAndVerifyMigrationsAsync(
        options,
        [
            "20260728101848_InitialCreateOracle",
            "20260728105917_AddUserRole"
        ]);

    await VerifyApiAsync("Oracle", schemaConnectionString);

    Console.WriteLine(useExistingSchema
        ? "PASS: Oracle migration/API testi mevcut TASKMANAGEMENT şemasında tamamlandı."
        : $"PASS: Oracle migration/API testi tamamlandı. Korunan test şeması: {schemaName}");
}

static async Task ApplyAndVerifyMigrationsAsync(
    DbContextOptions<ApplicationDbContext> options,
    IReadOnlyList<string> expectedMigrations)
{
    await using var context = new ApplicationDbContext(options);
    await context.Database.MigrateAsync();

    var applied = (await context.Database.GetAppliedMigrationsAsync()).ToArray();
    Assert(applied.SequenceEqual(expectedMigrations),
        $"Migration zinciri beklenenden farklı: {string.Join(", ", applied)}");
}

static async Task VerifyApiAsync(string provider, string connectionString)
{
    var port = GetAvailablePort();
    var apiUrl = $"http://127.0.0.1:{port}";
    var apiDll = Path.GetFullPath(Path.Combine(
        Directory.GetCurrentDirectory(),
        "..",
        "TaskManagement.API",
        "bin",
        "Release",
        "net9.0",
        "TaskManagement.API.dll"));

    using var process = StartApi(apiDll, apiUrl, provider, connectionString);

    try
    {
        using var client = new HttpClient { BaseAddress = new Uri(apiUrl) };
        await WaitForApiAsync(client, process);

        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"provider-{suffix}@test.local";
        var password = "ValidPassword123!";
        var registerPayload = $$"""
            {
              "username": "provider-{{suffix}}",
              "email": "{{email}}",
              "password": "{{password}}",
              "firstName": "Provider",
              "lastName": "Test"
            }
            """;

        var registerResponse = await client.PostAsync(
            "/api/auth/register",
            JsonContent(registerPayload));
        Assert(registerResponse.StatusCode == HttpStatusCode.Created,
            $"{provider} register başarısız: {await registerResponse.Content.ReadAsStringAsync()}");

        var loginResponse = await client.PostAsync(
            "/api/auth/login",
            JsonContent($$"""{"email":"{{email}}","password":"{{password}}"}"""));
        Assert(loginResponse.StatusCode == HttpStatusCode.OK,
            $"{provider} login başarısız: {await loginResponse.Content.ReadAsStringAsync()}");

        using var loginJson = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = loginJson.RootElement.GetProperty("data").GetString();
        Assert(!string.IsNullOrWhiteSpace(token), $"{provider} login token üretmedi.");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var taskResponse = await client.PostAsync(
            "/api/tasks",
            JsonContent("{\"title\":\"Provider migration testi\",\"priority\":3}"));
        Assert(taskResponse.StatusCode == HttpStatusCode.Created,
            $"{provider} task oluşturma başarısız: {await taskResponse.Content.ReadAsStringAsync()}");
    }
    finally
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
        }
    }
}

static Process StartApi(
    string apiDll,
    string apiUrl,
    string provider,
    string connectionString)
{
    var process = new Process
    {
        StartInfo = new ProcessStartInfo("dotnet", $"\"{apiDll}\" --urls {apiUrl}")
        {
            WorkingDirectory = Path.GetDirectoryName(apiDll)!,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }
    };

    process.StartInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
    process.StartInfo.Environment["DatabaseProvider"] = provider;
    process.StartInfo.Environment[$"ConnectionStrings__{provider}"] = connectionString;
    process.StartInfo.Environment["Logging__LogLevel__Default"] = "None";
    process.StartInfo.Environment["Logging__EventLog__LogLevel__Default"] = "None";
    process.Start();
    return process;
}

static async Task WaitForApiAsync(HttpClient client, Process process)
{
    for (var attempt = 0; attempt < 120; attempt++)
    {
        if (process.HasExited)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"API başlatılamadı: {error}");
        }

        try
        {
            var response = await client.GetAsync("/api/admin/users");
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return;
            }
        }
        catch (HttpRequestException)
        {
        }

        await Task.Delay(250);
    }

    throw new TimeoutException("API provider testi için zamanında başlamadı.");
}

static async Task ExecuteOracleAsync(OracleConnection connection, string sql)
{
    await using var command = connection.CreateCommand();
    command.CommandText = sql;
    await command.ExecuteNonQueryAsync();
}

static int GetAvailablePort()
{
    var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    var port = ((IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    return port;
}

static string? GetArgument(IReadOnlyList<string> args, string name)
{
    for (var index = 0; index < args.Count - 1; index++)
    {
        if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
        {
            return args[index + 1];
        }
    }

    return null;
}

static void EnsureIdentifier(string identifier, string pattern)
{
    if (!Regex.IsMatch(identifier, pattern))
    {
        throw new InvalidOperationException("Güvenli olmayan test hedefi adı.");
    }
}

static StringContent JsonContent(string json) =>
    new(json, Encoding.UTF8, "application/json");

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException($"FAIL: {message}");
    }
}
