using Devisions.Application.Locations;
using Devisions.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Devisions.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<ILocationsService, LocationsService>();

        return services;
    }
}