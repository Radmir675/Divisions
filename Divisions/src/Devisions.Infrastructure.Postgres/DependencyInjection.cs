using Devisions.Application.Interfaces;
using Devisions.Infrastructure.Postgres.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Devisions.Infrastructure.Postgres;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ILocationRepository, LocationRepository>();
        return services;
    }
}