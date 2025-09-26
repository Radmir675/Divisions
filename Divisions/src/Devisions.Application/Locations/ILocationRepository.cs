using Devisions.Domain.Location;

namespace Devisions.Application.Interfaces;

public interface ILocationRepository
{
    Task<Guid> AddAsync(Location location, CancellationToken cancellationToken);
}