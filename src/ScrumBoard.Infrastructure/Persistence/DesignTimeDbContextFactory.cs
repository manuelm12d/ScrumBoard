using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ScrumBoard.Infrastructure.Persistence;

/// <summary>
/// Used only by the "dotnet ef" tooling, which runs without the API host. It reads the same
/// environment variable as the application; the literal fallback points at the local docker database
/// and carries no production secret.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string LocalFallback =
        "Host=localhost;Port=5432;Database=scrumboard;Username=scrumboard;Password=scrumboard_dev_password";

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default") ?? LocalFallback;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .Options;

        return new AppDbContext(options);
    }
}
