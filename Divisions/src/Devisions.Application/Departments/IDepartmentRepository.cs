using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Domain.Department;
using Shared.Errors;

namespace Devisions.Application.Departments;

public interface IDepartmentRepository
{
    Task<Result<Department, Error>> GetByIdAsync(DepartmentId departmentId, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdWithLocationsAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken);

    Task<Result<Guid, Error>> AddAsync(Department value, CancellationToken cancellationToken);

    Task<UnitResult<Errors>> AllExistAndActiveAsync(
        IEnumerable<DepartmentId> departmentIds,
        CancellationToken cancellationToken);

    Task<Result<bool, Error>>
        IsIdentifierAlreadyExistsAsync(Identifier identifier, CancellationToken cancellationToken);

    Task<UnitResult<Error>> UpdateAsync(Department department, CancellationToken cancellationToken);

    Task<Result<IEnumerable<DepartmentId>, Error>> LockDescendants(
        Path parentPath, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdWithLock(
        DepartmentId departmentId,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> UpdateDepthDescendants(Path path, int deltaDepth, CancellationToken cancellationToken);

    Task<UnitResult<Error>> UpdatePathDescendants(Path oldPath, Path newPath, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> Delete(Department department, CancellationToken cancellationToken);
}