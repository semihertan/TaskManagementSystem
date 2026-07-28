using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TaskManagement.API.Data;
using TaskManagement.API.Entities;
using TaskManagement.API.Enums;
using TaskManagement.API.Services.Implementations;

var databaseName = $"task_management_rbac_test_{Guid.NewGuid():N}"[..34];
var maintenanceConnection = Environment.GetEnvironmentVariable("RBAC_TEST_POSTGRES")
    ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=";
var testConnectionBuilder = new NpgsqlConnectionStringBuilder(maintenanceConnection)
{
    Database = databaseName,
    Pooling = false
};

Process? apiProcess = null;

try
{
    EnsureTestDatabaseName(databaseName);
    await ExecuteMaintenanceSqlAsync($"CREATE DATABASE \"{databaseName}\"");

    var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseNpgsql(
            testConnectionBuilder.ConnectionString,
            options => options.MigrationsAssembly(
                "TaskManagement.API.Migrations.PostgreSql"))
        .Options;

    var adminId = Guid.NewGuid();
    var userOneId = Guid.NewGuid();
    var userTwoId = Guid.NewGuid();
    var userOneTaskId = Guid.NewGuid();
    var userTwoTaskId = Guid.NewGuid();
    var userOneCategoryId = Guid.NewGuid();
    var userTwoCategoryId = Guid.NewGuid();

    await using (var context = new ApplicationDbContext(dbOptions))
    {
        await context.Database.MigrateAsync();

        context.Users.AddRange(
            CreateUser(adminId, "admin", "admin@test.local", UserRole.Admin),
            CreateUser(userOneId, "user-one", "user-one@test.local", UserRole.User),
            CreateUser(userTwoId, "user-two", "user-two@test.local", UserRole.User));

        context.Tasks.AddRange(
            CreateTask(userOneTaskId, userOneId, "Birinci kullanıcının görevi"),
            CreateTask(userTwoTaskId, userTwoId, "İkinci kullanıcının görevi"));

        context.Categories.AddRange(
            CreateCategory(userOneCategoryId, userOneId, "Birinci kategori"),
            CreateCategory(userTwoCategoryId, userTwoId, "Ikinci kategori"));

        context.TaskComments.AddRange(
            CreateComment(userOneTaskId, userOneId, "Birinci yorum"),
            CreateComment(userTwoTaskId, userTwoId, "Ikinci yorum"));

        context.TaskAttachments.AddRange(
            CreateAttachment(userOneTaskId, "birinci.txt"),
            CreateAttachment(userTwoTaskId, "ikinci.txt"));

        await context.SaveChangesAsync();
    }

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

    apiProcess = StartApi(apiDll, apiUrl, testConnectionBuilder.ConnectionString);

    using var client = new HttpClient { BaseAddress = new Uri(apiUrl) };
    await WaitForApiAsync(client, apiProcess);

    var noTokenResponse = await client.GetAsync("/api/admin/users");
    Assert(noTokenResponse.StatusCode == HttpStatusCode.Unauthorized,
        "Token olmadan admin endpoint'i 401 dönmelidir.");

    var userToken = GenerateToken(CreateUser(
        userOneId,
        "user-one",
        "user-one@test.local",
        UserRole.User));
    var adminToken = GenerateToken(CreateUser(
        adminId,
        "admin",
        "admin@test.local",
        UserRole.Admin));

    SetBearer(client, userToken);
    var forbiddenResponse = await client.GetAsync("/api/admin/users");
    Assert(forbiddenResponse.StatusCode == HttpStatusCode.Forbidden,
        "User rolü admin endpoint'inde 403 almalıdır.");

    var userTasksResponse = await client.GetAsync("/api/tasks?page=1&pageSize=20");
    Assert(userTasksResponse.StatusCode == HttpStatusCode.OK,
        "User task listesi alınabilmelidir.");
    Assert(await GetTaskCountAsync(userTasksResponse) == 1,
        "User yalnızca kendi task kayıtlarını görmelidir.");

    var otherTaskResponse = await client.GetAsync($"/api/tasks/{userTwoTaskId}");
    Assert(otherTaskResponse.StatusCode == HttpStatusCode.NotFound,
        "User başka kullanıcının task detayını görememelidir.");

    var userCategoriesResponse = await client.GetAsync("/api/categories");
    Assert(userCategoriesResponse.StatusCode == HttpStatusCode.OK,
        "User kategori listesini alabilmelidir.");
    Assert(await GetDataArrayCountAsync(userCategoriesResponse) == 1,
        "User yalnizca kendi category kayitlarini gormelidir.");

    Assert((await client.GetAsync($"/api/categories/{userTwoCategoryId}")).StatusCode ==
           HttpStatusCode.NotFound,
        "User baska kullanicinin category detayini gormemelidir.");
    Assert((await client.GetAsync($"/api/tasks/{userTwoTaskId}/comments")).StatusCode ==
           HttpStatusCode.NotFound,
        "User baska kullanicinin comment kayitlarini gormemelidir.");
    Assert((await client.GetAsync($"/api/tasks/{userTwoTaskId}/attachments")).StatusCode ==
           HttpStatusCode.NotFound,
        "User baska kullanicinin attachment kayitlarini gormemelidir.");

    SetBearer(client, adminToken);
    var adminUsersResponse = await client.GetAsync("/api/admin/users");
    Assert(adminUsersResponse.StatusCode == HttpStatusCode.OK,
        "Admin kullanıcı listesini alabilmelidir.");

    Assert(await GetDataArrayCountAsync(adminUsersResponse) == 3,
        "PostgreSQL migration zinciri guvensiz demo kullanici olusturmamalidir.");

    var adminTasksResponse = await client.GetAsync("/api/tasks?page=1&pageSize=20");
    Assert(adminTasksResponse.StatusCode == HttpStatusCode.OK,
        "Admin task listesi alınabilmelidir.");
    Assert(await GetTaskCountAsync(adminTasksResponse) == 2,
        "Admin tüm kullanıcıların task kayıtlarını görmelidir.");

    var adminCategoriesResponse = await client.GetAsync("/api/categories");
    Assert(adminCategoriesResponse.StatusCode == HttpStatusCode.OK &&
           await GetDataArrayCountAsync(adminCategoriesResponse) == 2,
        "Admin tum kullanicilarin category kayitlarini gormelidir.");

    var adminCommentsResponse = await client.GetAsync($"/api/tasks/{userTwoTaskId}/comments");
    Assert(adminCommentsResponse.StatusCode == HttpStatusCode.OK &&
           await GetRootArrayCountAsync(adminCommentsResponse) == 1,
        "Admin tum kullanicilarin comment kayitlarini gormelidir.");

    var adminAttachmentsResponse =
        await client.GetAsync($"/api/tasks/{userTwoTaskId}/attachments");
    Assert(adminAttachmentsResponse.StatusCode == HttpStatusCode.OK &&
           await GetRootArrayCountAsync(adminAttachmentsResponse) == 1,
        "Admin tum kullanicilarin attachment kayitlarini gormelidir.");

    var adminUserDetailResponse = await client.GetAsync($"/api/admin/users/{userOneId}");
    Assert(adminUserDetailResponse.StatusCode == HttpStatusCode.OK,
        "Admin kullanici detayini alabilmelidir.");
    using (var userDetailJson = JsonDocument.Parse(
               await adminUserDetailResponse.Content.ReadAsStringAsync()))
    {
        Assert(!userDetailJson.RootElement.GetProperty("data")
                .TryGetProperty("passwordHash", out _),
            "Admin kullanici response'u PasswordHash icermemelidir.");
    }

    var promoteResponse = await client.PatchAsync(
        $"/api/admin/users/{userTwoId}/role",
        JsonContent("{\"role\":1}"));
    Assert(promoteResponse.StatusCode == HttpStatusCode.OK &&
           await GetDataIntAsync(promoteResponse, "role") == 1,
        "Admin kullanici rolunu Admin yapabilmelidir.");

    var restoreRoleResponse = await client.PatchAsync(
        $"/api/admin/users/{userTwoId}/role",
        JsonContent("{\"role\":0}"));
    Assert(restoreRoleResponse.StatusCode == HttpStatusCode.OK &&
           await GetDataIntAsync(restoreRoleResponse, "role") == 0,
        "Admin kullanici rolunu User yapabilmelidir.");

    var deactivateResponse = await client.PatchAsync(
        $"/api/admin/users/{userTwoId}/status",
        JsonContent("{\"isActive\":false}"));
    Assert(deactivateResponse.StatusCode == HttpStatusCode.OK &&
           !await GetDataBoolAsync(deactivateResponse, "isActive"),
        "Admin kullaniciyi pasif yapabilmelidir.");

    var activateResponse = await client.PatchAsync(
        $"/api/admin/users/{userTwoId}/status",
        JsonContent("{\"isActive\":true}"));
    Assert(activateResponse.StatusCode == HttpStatusCode.OK &&
           await GetDataBoolAsync(activateResponse, "isActive"),
        "Admin kullaniciyi yeniden aktif yapabilmelidir.");

    var demoteResponse = await client.PatchAsync(
        $"/api/admin/users/{adminId}/role",
        JsonContent("{\"role\":0}"));
    Assert(demoteResponse.StatusCode == HttpStatusCode.Conflict,
        "Son aktif adminin rolü düşürülememelidir.");

    client.DefaultRequestHeaders.Authorization = null;
    var registerPayload = $$"""
        {
          "username": "role-injection",
          "email": "role-injection@test.local",
          "password": "ValidPassword123!",
          "firstName": "Role",
          "lastName": "Injection",
          "role": 1
        }
        """;
    var registerResponse = await client.PostAsync("/api/auth/register", JsonContent(registerPayload));
    Assert(registerResponse.StatusCode == HttpStatusCode.Created,
        "Role alanı içeren register isteği güvenli biçimde işlenmelidir.");

    using var registerJson = JsonDocument.Parse(await registerResponse.Content.ReadAsStringAsync());
    Assert(registerJson.RootElement.GetProperty("data").GetProperty("role").GetInt32() == 0,
        "Register isteğindeki Admin rolü dikkate alınmamalıdır.");

    Console.WriteLine("PASS: Token olmadan admin endpoint'i 401 döndü.");
    Console.WriteLine("PASS: User admin endpoint'inde 403 aldı.");
    Console.WriteLine("PASS: User task izolasyonu ve başka kullanıcı task detayı korundu.");
    Console.WriteLine("PASS: Admin tüm task ve kullanıcı kayıtlarını görüntüledi.");
    Console.WriteLine("PASS: Son aktif admin koruması çalıştı.");
    Console.WriteLine("PASS: Register rol enjeksiyonu User rolüne zorlandı.");
    Console.WriteLine("PASS: PostgreSQL migration zinciri gecici veritabanina uygulandi.");
    Console.WriteLine("PASS: Category, comment ve attachment rol izolasyonu dogrulandi.");
}
finally
{
    if (apiProcess is { HasExited: false })
    {
        apiProcess.Kill(entireProcessTree: true);
        await apiProcess.WaitForExitAsync();
    }

    NpgsqlConnection.ClearAllPools();

    if (Regex.IsMatch(databaseName, "^task_management_rbac_test_[a-f0-9]+$"))
    {
        try
        {
            await ExecuteMaintenanceSqlAsync(
                $"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)");
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Geçici test veritabanı temizlenemedi: {exception.Message}");
        }
    }
}

async Task ExecuteMaintenanceSqlAsync(string sql)
{
    await using var connection = new NpgsqlConnection(maintenanceConnection);
    await connection.OpenAsync();
    await using var command = new NpgsqlCommand(sql, connection);
    await command.ExecuteNonQueryAsync();
}

static void EnsureTestDatabaseName(string name)
{
    if (!Regex.IsMatch(name, "^task_management_rbac_test_[a-f0-9]+$"))
    {
        throw new InvalidOperationException("Güvenli olmayan test veritabanı adı.");
    }
}

static User CreateUser(Guid id, string username, string email, UserRole role) => new()
{
    Id = id,
    Username = username,
    Email = email,
    PasswordHash = BCrypt.Net.BCrypt.HashPassword("ValidPassword123!"),
    FirstName = username,
    LastName = "Test",
    Role = role,
    IsActive = true,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

static TaskItem CreateTask(Guid id, Guid userId, string title) => new()
{
    Id = id,
    UserId = userId,
    Title = title,
    Priority = Priority.Normal,
    Status = TaskItemStatus.Pending,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

static Category CreateCategory(Guid id, Guid userId, string name) => new()
{
    Id = id,
    UserId = userId,
    Name = name,
    Color = "#007bff",
    CreatedAt = DateTime.UtcNow
};

static TaskComment CreateComment(Guid taskId, Guid userId, string content) => new()
{
    Id = Guid.NewGuid(),
    TaskId = taskId,
    UserId = userId,
    Content = content,
    CreatedAt = DateTime.UtcNow
};

static TaskAttachment CreateAttachment(Guid taskId, string fileName) => new()
{
    Id = Guid.NewGuid(),
    TaskId = taskId,
    FileName = fileName,
    FilePath = Path.Combine("Uploads", fileName),
    FileSize = 1,
    ContentType = "text/plain",
    UploadedAt = DateTime.UtcNow
};

static int GetAvailablePort()
{
    var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    var port = ((IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    return port;
}

static Process StartApi(string apiDll, string apiUrl, string connectionString)
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
    process.StartInfo.Environment["DatabaseProvider"] = "PostgreSql";
    process.StartInfo.Environment["ConnectionStrings__PostgreSql"] = connectionString;
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

    throw new TimeoutException("API güvenlik testi için zamanında başlamadı.");
}

static string GenerateToken(User user)
{
    const string jwtKey = "BuEnAz32KarakterUzunlugundaGizliBirAnahtar123!";
    const string jwtIssuer = "TaskManagement.API";
    const string jwtAudience = "TaskManagement.Client";

    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = jwtKey,
            ["Jwt:Issuer"] = jwtIssuer,
            ["Jwt:Audience"] = jwtAudience,
            ["Jwt:ExpireMinutes"] = "10"
        })
        .Build();

    return new JwtService(configuration).GenerateToken(user);
}

static void SetBearer(HttpClient client, string token)
{
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
}

static StringContent JsonContent(string json) =>
    new(json, Encoding.UTF8, "application/json");

static async Task<int> GetTaskCountAsync(HttpResponseMessage response)
{
    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    return json.RootElement
        .GetProperty("data")
        .GetProperty("items")
        .GetArrayLength();
}

static async Task<int> GetDataArrayCountAsync(HttpResponseMessage response)
{
    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    return json.RootElement.GetProperty("data").GetArrayLength();
}

static async Task<int> GetRootArrayCountAsync(HttpResponseMessage response)
{
    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    return json.RootElement.GetArrayLength();
}

static async Task<int> GetDataIntAsync(HttpResponseMessage response, string propertyName)
{
    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    return json.RootElement.GetProperty("data").GetProperty(propertyName).GetInt32();
}

static async Task<bool> GetDataBoolAsync(HttpResponseMessage response, string propertyName)
{
    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    return json.RootElement.GetProperty("data").GetProperty(propertyName).GetBoolean();
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException($"FAIL: {message}");
    }
}
