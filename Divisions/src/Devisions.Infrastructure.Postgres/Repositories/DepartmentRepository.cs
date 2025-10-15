using CSharpFunctionalExtensions;
using Devisions.Application.Departments;
using Devisions.Domain.Department;
using Devisions.Domain.Position;
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

    public async Task<Result<Department, Error>> GetByIdAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        try
        {
            var department = await _dbContext.Departments.FindAsync(departmentId, cancellationToken);
            if (department == null)
                return Error.NotFound("department.repository", "Department not found", null);

            return department;
        }
        catch (Exception e)
        {
            _logger.LogError(e?.InnerException?.Message);
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
            _logger.LogError(e?.InnerException?.Message);
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
            _logger.LogError(e?.InnerException?.Message);
            return Error.Failure(
                "department.repository.GetAllAsync",
                "Departments is not founded in repository");
        }
    }

    public async Task<UnitResult<Error>> AddPositionAsync(
        Guid[] departmentsId,
        Guid positionId,
        CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var departmentId in departmentsId)
            {
                var departmentDbResult = await GetByIdAsync(departmentId, cancellationToken);
                if (departmentDbResult.IsFailure)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return departmentDbResult.Error;
                }

                var departmentPosition = new DepartmentPosition(
                    Guid.NewGuid(),
                    new DepartmentId(departmentId),
                    new PositionId(positionId));
            }

            await transaction.CommitAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            return Error.Failure("add.position.db", "Position could not be added in repository");
        }
    }
}