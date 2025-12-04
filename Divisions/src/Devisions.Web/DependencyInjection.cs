using Devisions.Application;
using Devisions.Application.Services;
using Devisions.Infrastructure.Postgres;
using Devisions.Infrastructure.Postgres.BackgroundServices;
using Devisions.Infrastructure.Postgres.Seeder;
using Devisions.Web.EndPointResults;
using Microsoft.Extensions.Caching.Distributed;

namespace Devisions.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddWebDependencies()
            .AddApplication()
            .AddInfrastructure()
            .AddDistributedCache(configuration);

        return services;
    }

    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.OperationFilter<EndPointResultOperationFilter>();
        });
        services.AddScoped<ISeeder, DevisionsSeeder>();
        services.AddScoped<IDivisionCleanerService, DivisionCleanerService>();

        return services;
    }

    private static IServiceCollection AddDistributedCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            string connectionString = configuration.GetConnectionString("Redis")
                                      ?? throw new ArgumentNullException(nameof(connectionString));
            options.Configuration = connectionString;
        });
        services.AddSingleton<ICacheService, DistributedCacheService>();
        return services;
    }
}