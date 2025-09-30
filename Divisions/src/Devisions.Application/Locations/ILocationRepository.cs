using CSharpFunctionalExtensions;
using Devisions.Domain.Location;
using Shared.Failures;

namespace Devisions.Application.Locations;

public interface ILocationRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken);
}