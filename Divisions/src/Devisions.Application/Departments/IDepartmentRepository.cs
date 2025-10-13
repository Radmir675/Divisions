using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Domain.Department;
using Shared.Errors;

namespace Devisions.Application.Departments;

public interface IDepartmentRepository
{
    Task<Result<Department, Error>> GetByIdAsync(Guid departmentId, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> Add(Department value, CancellationToken cancellationToken);
}