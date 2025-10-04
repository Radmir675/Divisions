using Devisions.Application.Abstractions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Devisions.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        var assembly = typeof(DependencyInjection).Assembly;

        services.Scan(scan => scan.FromAssemblies([assembly])
            .AddClasses(class1 => class1
                .AssignableToAny(typeof(ICommandHandler<,>), typeof(ICommandHandler<>)))
            .AsSelfWithInterfaces()
            .WithTransientLifetime());

        return services;
    }
}