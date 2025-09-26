using Devisions.Contracts;

namespace Devisions.Application.Locations;

public interface ILocationsService
{
    Task<Guid> CreateAsync(CreateLocationDto dto, CancellationToken cancellationToken);
}