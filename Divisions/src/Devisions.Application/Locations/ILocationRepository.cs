using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Domain.Location;
using Shared.Errors;

namespace Devisions.Application.Locations;

public interface ILocationRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken);

    Task<UnitResult<Errors>> ExistsByIdsAsync(IEnumerable<LocationId> locationsId, CancellationToken cancellationToken);
}