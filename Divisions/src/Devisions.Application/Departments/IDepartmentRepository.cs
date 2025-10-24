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

    Task<Result<Guid, Error>> AddAsync(Department value, CancellationToken cancellationToken);

    Task<UnitResult<Errors>> AllExistAndActiveAsync(
        IEnumerable<DepartmentId> departmentIds,
        CancellationToken cancellationToken);

    Task<Result<bool, Error>> IsIdentifierFreeAsync(Identifier identifier, CancellationToken cancellationToken);
}