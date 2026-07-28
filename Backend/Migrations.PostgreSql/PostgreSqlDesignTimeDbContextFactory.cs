using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TaskManagement.API.Data;

namespace TaskManagement.API.Migrations.PostgreSql;

public sealed class PostgreSqlDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = GetArgument(args, "--connection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql")
            ?? "Host=localhost;Port=5432;Database=task_management_db;Username=postgres;Password=";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString, provider =>
                provider.MigrationsAssembly(typeof(PostgreSqlDesignTimeDbContextFactory)
                    .Assembly.FullName))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static string? GetArgument(IReadOnlyList<string> args, string name)
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
}
