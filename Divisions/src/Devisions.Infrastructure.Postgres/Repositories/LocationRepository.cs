using CSharpFunctionalExtensions;
using Devisions.Application.Locations;
using Devisions.Domain.Location;
using Microsoft.EntityFrameworkCore;
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

    public async Task<Result<bool, Error>> ExistsByIdsAsync(
        IEnumerable<Guid> locationsId,
        CancellationToken cancellationToken)
    {
        try
        {
            foreach (var locationId in locationsId)
            {
                var isIdExist = await _dbContext
                    .Locations
                    .AnyAsync(x => x.Id.Value == locationId, cancellationToken);
                if (!isIdExist)
                {
                    return Error.NotFound("locationRepository.ExistsByIdAsync",
                        "location not found",
                        locationId);
                }
            }

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e?.InnerException.Message);
            return Error.Failure("locationRepository.ExistsByIdAsync",
                "Location could not be found");
        }
    }
}