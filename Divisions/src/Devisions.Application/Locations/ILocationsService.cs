using CSharpFunctionalExtensions;
using Devisions.Contracts.Locations;
using Shared.Errors;

namespace Devisions.Application.Locations;

public interface ILocationsService
{
    Task<Result<Guid, Errors>> CreateAsync(CreateLocationDto dto, CancellationToken cancellationToken);
}