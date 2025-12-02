using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Domain.Department;
using Devisions.Domain.Location;
using Shared.Errors;

namespace Devisions.Application.Locations;

public interface ILocationRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken);

    Task<UnitResult<Errors>> ExistsAsync(IEnumerable<LocationId> locationsId, CancellationToken cancellationToken);

    Task<UnitResult<Errors>> AreAllActiveAsync(
        IEnumerable<LocationId> locationsId,
        CancellationToken cancellationToken);

    Task<Result<IEnumerable<Location>, Error>> GetByIdsAsync(
        IEnumerable<LocationId> locationIds,
        CancellationToken cancellationToken);

    Task<Result<IEnumerable<LocationId>, Error>> GetExclusiveToDepartmentAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken);

    Task<IEnumerable<Location>> GetRemovableAsync(CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeleteAsync(
        IEnumerable<LocationId> locationIds,
        CancellationToken cancellationToken);
}