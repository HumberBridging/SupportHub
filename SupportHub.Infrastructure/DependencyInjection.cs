using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportHub.Application.Contracts;

namespace SupportHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register your infrastructure services here

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<SupportHubDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<SupportHubDbContext>());

        return services;
    }
}
