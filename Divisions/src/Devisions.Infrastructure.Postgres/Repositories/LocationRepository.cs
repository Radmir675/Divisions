using CSharpFunctionalExtensions;
using Devisions.Application.Locations;
using Devisions.Domain.Location;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Infrastructure.Postgres.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<LocationRepository> _logger;

    public LocationRepository(AppDbContext dbContext, ILogger<LocationRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Locations.Add(location);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e?.InnerException.Message);
            return Error.Failure("locationRepository.AddAsync",
                "Location could not be added in repository");
        }

        return location.Id.Value;
    }
}