using CSharpFunctionalExtensions;
using Devisions.Domain.Location;

namespace Devisions.Application.Locations;

public interface ILocationRepository
{
    Task<Result<Guid>> AddAsync(Location location, CancellationToken cancellationToken);
}