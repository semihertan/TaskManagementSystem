using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TaskManagement.API.Data;

namespace TaskManagement.API.Migrations.Oracle;

public sealed class OracleDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = GetArgument(args, "--connection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Oracle")
            ?? "User Id=system;Password=;Data Source=localhost:1521/FREEPDB1";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseOracle(connectionString, provider =>
                provider.MigrationsAssembly(typeof(OracleDesignTimeDbContextFactory)
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
