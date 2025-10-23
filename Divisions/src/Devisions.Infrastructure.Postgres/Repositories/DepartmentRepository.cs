using CSharpFunctionalExtensions;
using Devisions.Application.Departments;
using Devisions.Domain.Department;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Infrastructure.Postgres.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<LocationRepository> _logger;

    public DepartmentRepository(AppDbContext dbContext, ILogger<LocationRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Department, Error>> GetByIdAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var department =
                await _dbContext.Departments.FirstOrDefaultAsync(x => x.Id == departmentId, cancellationToken);
            if (department == null)
                return Error.NotFound("department.repository", "Department not found", null);

            return department;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Error.Failure(
                "department.repository.getById",
                "department could not be founded");
        }
    }

    public async Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Departments.Add(department);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return department.Id.Value;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Error.Failure(
                "department.repository.AddAsync",
                "Department could not be added in repository");
        }
    }

    public async Task<UnitResult<Errors>> AllActiveAsync(
        IEnumerable<DepartmentId> departmentIds,
        CancellationToken cancellationToken)
    {
        List<Error> errors = [];

        foreach (var departmentId in departmentIds)
        {
            var department = await _dbContext.Departments
                .FindAsync(departmentId, cancellationToken);
            if (department == null || !department.IsActive)
            {
                errors.Add(Error.NotFound("department.", "Not found active department", departmentId.Value));
            }
        }

        return errors.Any() ? new Errors(errors) : UnitResult.Success<Errors>();
    }

    public async Task<Result<bool, Error>> IsIdentifierFreeAsync(
        Identifier identifier,
        CancellationToken cancellationToken)
    {
        try
        {
            bool result = await _dbContext.Departments.AnyAsync(
                x => x.Identifier.Identify == identifier.Identify,
                cancellationToken);
            return !result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Error.Failure("department.repository.isIdentifierFreeAsync",
                "Repository error");
        }
    }
}