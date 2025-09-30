using CSharpFunctionalExtensions;
using Devisions.Contracts;
using Shared.Failures;

namespace Devisions.Application.Locations;

public interface ILocationsService
{
    Task<Result<Guid, Failure>> CreateAsync(CreateLocationDto dto, CancellationToken cancellationToken);
}