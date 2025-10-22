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

    public async Task<Result<IEnumerable<Department>, Error>> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            var departments = await _dbContext.Departments.ToListAsync(cancellationToken);
            return departments;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Error.Failure(
                "department.repository.GetAllAsync",
                "Departments is not founded in repository");
        }
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