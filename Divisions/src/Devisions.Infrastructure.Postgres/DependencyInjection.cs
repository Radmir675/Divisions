using Microsoft.Extensions.DependencyInjection;

namespace Devisions.Infrastructure.Postgres;

public static class DependencyInjection
{
    // public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    // {
    //     services.AddScoped<ILocationRepository, LocationRepository>();
    //     services.AddScoped<IDepartmentRepository, DepartmentRepository>();
    //     services.AddScoped<IPositionsRepository, PositionsRepository>();
    //     return services;
    // }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(classes => classes
                .Where(type => type.Name.EndsWith("Repository")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}