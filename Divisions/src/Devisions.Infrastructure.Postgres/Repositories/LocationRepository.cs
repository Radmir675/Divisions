using CSharpFunctionalExtensions;
using Devisions.Application.Locations;
using Devisions.Domain.Location;
using Devisions.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
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
        catch (DbUpdateException ex)when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } &&
                pgEx.ConstraintName.Contains("name", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogError(
                    pgEx,
                    "Database update error while creating location with name:{name}",
                    location.Name);
                return GeneralErrors.AlreadyExist(nameof(location.Name));
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation cancelled");
            return GeneralErrors.CanceledOperation();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while creating location with location id:{location}",
                location.Id);
            return GeneralErrors.DatabaseError();
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

    public async Task<Result<IEnumerable<Location>, Error>> GetByIds(
        IEnumerable<LocationId> locationIds,
        CancellationToken cancellationToken)
    {
        var result = await _dbContext.Locations
            .Include(x => x.DepartmentLocations)
            .Where(x => locationIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (result.Any() == false)
        {
            return GeneralErrors.NotFoundInDatabase();
        }

        return result;
    }
}