using Devisions.Application.Database;
using Devisions.Infrastructure.Postgres.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Devisions.Infrastructure.Postgres;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(classes => classes
                .Where(type => type.Name.EndsWith("Repository")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddScoped<ITransactionManager, TransactionManager>();

        return services;
    }
}