using Devisions.Infrastructure.Postgres.Seeder;

namespace Devisions.Web.Extensions;

public static class SeederExtension
{
    public static async Task<IServiceProvider> RunSeeding(this IServiceProvider services)
    {
        var cancellationToken = CancellationToken.None;
        using var scope = services.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<ISeeder>();
        foreach (var seeder in seeders)
        {
            await seeder.CreateAsync(cancellationToken);
        }

        return services;
    }
}