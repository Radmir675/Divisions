using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Domain.Department;
using Shared.Errors;

namespace Devisions.Application.Departments;

public interface IDepartmentRepository
{
    Task<Result<Department, Error>> GetByIdAsync(DepartmentId departmentId, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByAsync(
        Expression<Func<Department, bool>> predicate,
        CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdIncludingLocationsAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken);

    Task<Result<Guid, Error>> AddAsync(Department value, CancellationToken cancellationToken);

    Task<UnitResult<Errors>> AreAllActiveAsync(
        IEnumerable<DepartmentId> departmentIds,
        CancellationToken cancellationToken);

    Task<Result<IEnumerable<DepartmentId>, Error>> LockDescendants(
        Path parentPath, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdWithLock(
        DepartmentId departmentId,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> UpdateDescendantsDepthAsync(Path path, int deltaDepth, CancellationToken cancellationToken);

    Task<UnitResult<Error>> UpdateDescendantsPathAsync(Path oldPath, Path newPath, CancellationToken cancellationToken);

    Task<Result<IEnumerable<Guid>, Error>> DeleteAsync(
        IEnumerable<Department> departments,
        CancellationToken cancellationToken);

    Task<IEnumerable<Department>> GetRemovableAsync(CancellationToken cancellationToken);
}