using CSharpFunctionalExtensions;
using Devisions.Application.Locations;
using Devisions.Domain.Location;
using Devisions.Infrastructure.Postgres.Database;
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
            _logger.LogError(e, e.Message);
            return Error.Failure(
                "locationRepository.AddAsync",
                "Location could not be added in repository");
        }

        return location.Id.Value;
    }

    public async Task<UnitResult<Errors>> ExistsByIdsAsync(
        IEnumerable<LocationId> locationsId,
        CancellationToken cancellationToken)
    {
        List<Error> errors = [];
        var existingIds = await _dbContext.Locations
            .Where(x => locationsId
                .Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var invalidLocations = locationsId
            .Except(existingIds)
            .ToList();

        if (invalidLocations.Any())
        {
            invalidLocations.ForEach(locationId => errors.Add(Error.NotFound(
                "locationRepository.ExistsByIdAsync",
                "location not found",
                locationId.Value)));
        }

        return errors.Any() ? new Errors(errors) : UnitResult.Success<Errors>();
    }

    public async Task<UnitResult<Errors>> AllExistsAndActiveAsync(
        IEnumerable<LocationId> locationsId,
        CancellationToken cancellationToken)
    {
        List<Error> errors = [];
        var activeLocationsId = await _dbContext.Locations
            .Where(x => locationsId
                .Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var invalidLocations = locationsId.Except(activeLocationsId).ToList();

        if (invalidLocations.Any())
        {
            invalidLocations.ForEach(locationId => errors.Add(Error.NotFound(
                "locationRepository.ExistsByIdAsync",
                "location not found or inactive",
                locationId.Value)));
        }

        return errors.Any() ? new Errors(errors) : UnitResult.Success<Errors>();
    }
}