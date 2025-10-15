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
    Task<Result<Department, Error>> GetByIdAsync(Guid departmentId, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> AddAsync(Department value, CancellationToken cancellationToken);

    Task<Result<IEnumerable<Department>, Error>> GetAllAsync(CancellationToken cancellationToken);
}