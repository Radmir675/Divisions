using Devisions.Domain.Location;
using Devisions.Infrastructure.Postgres.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Divisions.IntegrationTests.Share;

public class LocationCreator
{
    private readonly IServiceProvider _services;

    public LocationCreator(IServiceProvider serviceProvider)
    {
        _services = serviceProvider;
    }

    public async Task<LocationId> CreateAsync(string locationName, CancellationToken cancellationToken)
    {
        await using var scope = _services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var location = Location.Create(
            locationName,
            Address.Create("Russia", "Moscow", "Lenina", 1, null).Value,
            true,
            Timezone.Create("Europe/Moscow").Value);

        dbContext.Locations.Add(location.Value);
        await dbContext.SaveChangesAsync(cancellationToken);
        var locationId = location.Value.Id;

        return locationId;
    }
}