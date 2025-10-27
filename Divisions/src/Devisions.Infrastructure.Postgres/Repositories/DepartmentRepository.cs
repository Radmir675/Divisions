using CSharpFunctionalExtensions;
using Devisions.Application.Departments;
using Devisions.Domain.Department;
using Devisions.Infrastructure.Postgres.Database;
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
        var department =
            await _dbContext.Departments
                .FirstOrDefaultAsync(x => x.Id == departmentId, cancellationToken);
        if (department == null)
            return Error.NotFound("department.repository", "Department not found", null);

        return department;
    }

    public async Task<Result<Department, Error>> GetByIdWithLocationsAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        var department =
            await _dbContext.Departments
                .Include(x => x.DepartmentLocations)
                .FirstOrDefaultAsync(x => x.Id == departmentId, cancellationToken);
        if (department == null)
            return Error.NotFound("department.repository", "Department not found", null);

        return department;
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

    public async Task<UnitResult<Errors>> AllExistAndActiveAsync(
        IEnumerable<DepartmentId> departmentIds,
        CancellationToken cancellationToken)
    {
        List<Error> errors = [];
        var activeDepartmentIds = await _dbContext.Departments
            .Where(x => departmentIds
                .Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var invalidDepartments = departmentIds.Except(activeDepartmentIds).ToList();

        if (invalidDepartments.Any())
        {
            invalidDepartments.ForEach(locationId => errors.Add(Error.NotFound(
                "locationRepository.ExistsByIdAsync",
                "location not found or inactive",
                locationId.Value)));
        }

        return errors.Any() ? new Errors(errors) : UnitResult.Success<Errors>();
    }

    public async Task<UnitResult<Error>> UpdateAsync(Department department, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Departments.Update(department);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Error.Failure(
                "department.repository.UpdateAsync",
                "Department is not updated");
        }

        return UnitResult.Success<Error>();
    }

    public async Task<Result<bool, Error>> IsIdentifierAlreadyExistsAsync(
        Identifier identifier,
        CancellationToken cancellationToken)
    {
            bool result = await _dbContext.Departments.AnyAsync(
                x => x.Identifier.Identify == identifier.Identify,
                cancellationToken);
            return result;
    }
}