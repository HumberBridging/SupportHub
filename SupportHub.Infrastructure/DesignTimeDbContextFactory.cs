using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SupportHub.Infrastructure;

/// <summary>
/// Enables EF Core design-time tooling (migrations) to construct the context
/// without running the host. The connection string here is only used to build
/// the model/migrations, never at runtime.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SupportHubDbContext>
{
    public SupportHubDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SupportHubDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("SUPPORTHUB_MIGRATIONS_CONNECTION")
            ?? "Server=Armoury;Database=SupportHubCore;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString);

        return new SupportHubDbContext(optionsBuilder.Options);
    }
}
