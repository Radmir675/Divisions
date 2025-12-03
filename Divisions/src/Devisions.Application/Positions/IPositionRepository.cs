using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Domain.Department;
using Devisions.Domain.Position;
using Shared.Errors;

namespace Devisions.Application.Positions;

public interface IPositionsRepository
{
    Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken);

    Task<Result<bool, Error>> IsNameAvailableAsync(PositionName name, CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeleteAsync(
        IEnumerable<PositionId> positionIds,
        CancellationToken cancellationToken);

    Task<Result<IEnumerable<PositionId>, Error>> GetPositionsExclusiveToAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken);

    Task<Result<IEnumerable<Position>, Error>> GetByIdsAsync(
        IEnumerable<PositionId> positionIds,
        CancellationToken cancellationToken);

    Task<IEnumerable<Position>> GetRemovableAsync(CancellationToken cancellationToken);
}