using Devisions.Application.Interfaces;
using Devisions.Domain.Location;

namespace Devisions.Infrastructure.Postgres.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _dbContext;

    public LocationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> AddAsync(Location location, CancellationToken cancellationToken)
    {
        _dbContext.Locations.Add(location);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return location.Id;
    }
}