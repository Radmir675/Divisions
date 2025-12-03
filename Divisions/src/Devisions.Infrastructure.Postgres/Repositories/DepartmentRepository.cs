using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Dapper;
using Devisions.Application.Departments;
using Devisions.Domain.Department;
using Devisions.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Errors;
using Path = Devisions.Domain.Department.Path;

namespace Devisions.Infrastructure.Postgres.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<LocationRepository> _logger;

    public DepartmentRepository(
        AppDbContext dbContext,
        ILogger<LocationRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Department, Error>> GetByAsync(
        Expression<Func<Department, bool>> predicate,
        CancellationToken cancellationToken)
    {
        var department =
            await _dbContext.Departments
                .FirstOrDefaultAsync(predicate, cancellationToken);
        if (department == null)
            return Error.NotFound("department.repository", "Department not found", null);

        return department;
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

    public async Task<Result<Department, Error>> GetByIdIncludingLocationsAsync(
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
        catch (DbUpdateException ex)when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } &&
                pgEx.ConstraintName.Contains(
                    nameof(department.Identifier),
                    StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogError(
                    pgEx,
                    "Database update error while creating department with identifier:{identifier}",
                    department.Identifier.Identify);

                return GeneralErrors.AlreadyExist(nameof(department.Identifier.Identify));
            }

            return GeneralErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation cancelled");
            return GeneralErrors.CanceledOperation();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating department");
            return Error.Failure(
                "department.repository.AddAsync",
                "Department could not be added in repository");
        }
    }

    public async Task<UnitResult<Errors>> AreAllActiveAsync(
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

    public async Task<Result<IEnumerable<DepartmentId>, Error>> LockDescendants(
        Path rootPath,
        CancellationToken cancellationToken)
    {
        try
        {
            const string dapperSql = """
                                     SELECT id
                                     FROM departments
                                     WHERE path <@ @rootPath::ltree 
                                     AND path!=@rootPath::ltree
                                     ORDER BY depth
                                     FOR UPDATE
                                     """;

            var connection = _dbContext.Database.GetDbConnection();
            var departmentIds = (await connection.QueryAsync<Guid>(
                    dapperSql,
                    new { rootPath = rootPath.PathValue }))
                .Select(x => new DepartmentId(x))
                .ToList();

            return departmentIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to lock descendants");
            return Error.Failure("fail.lock.descendance.", "Failed to lock descendants");
        }
    }

    public async Task<Result<Department, Error>> GetByIdWithLock(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlAsync(
                $"SELECT * FROM departments WHERE id={departmentId.Value} FOR UPDATE",
                cancellationToken);

            var department =
                await _dbContext.Departments
                    .FirstOrDefaultAsync(x => x.Id == departmentId, cancellationToken);
            if (department is null)
            {
                return Error.NotFound(
                    "department.repository.lock",
                    "Department not found",
                    departmentId.Value);
            }

            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to lock by id");
            return Error.Failure("fail.lock", "Failed to lock by id");
        }
    }

    public async Task<UnitResult<Error>> UpdateDescendantsPathAsync(
        Path oldPath,
        Path newPath,
        CancellationToken cancellationToken)
    {
        try
        {
            FormattableString query = $@"UPDATE departments
                           SET path=CASE
                               WHEN {newPath.PathValue} = '' 
                                   THEN subpath(path, 1)
                                   ELSE {newPath.PathValue}::ltree || subpath(path, nlevel({oldPath.PathValue}::ltree))
                                   END
                           WHERE path <@ {oldPath.PathValue}::ltree AND path != {oldPath.PathValue}::ltree";

            await _dbContext.Database.ExecuteSqlInterpolatedAsync(query, cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update path");
            return UnitResult.Failure(Error.Failure("fail.update.path.", "Failed to update path"));
        }
    }

    public async Task<UnitResult<Error>> UpdateDescendantsDepthAsync(
        Path path,
        int deltaDepth,
        CancellationToken cancellationToken)
    {
        try
        {
            var dep = _dbContext.Departments.ToList();
            FormattableString query =
                $@"UPDATE departments
                         SET depth =depth + {deltaDepth}
                         WHERE path <@ {path.PathValue}::ltree 
                         AND path != {path.PathValue}::ltree";
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(query, cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update path");
            return UnitResult.Failure(Error.Failure("fail.update.depth.", "Failed to update depth"));
        }
    }

    public async Task<Result<IEnumerable<Guid>, Error>> DeleteAsync(
        IEnumerable<Department> departments,
        CancellationToken cancellationToken)
    {
        try
        {
            var departmentsList = departments.ToList();

            _dbContext.DepartmentPositions
                .RemoveRange(departmentsList.SelectMany(d => d.DepartmentPositions));
            _dbContext.DepartmentLocations
                .RemoveRange(departmentsList.SelectMany(d => d.DepartmentLocations));

            _dbContext.Departments.RemoveRange(departmentsList);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return departmentsList.Select(d => d.Id.Value).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete departments.");
            return GeneralErrors.DatabaseError($"Failed to delete departments");
        }
    }

    public async Task<IEnumerable<Department>> GetRemovableAsync(CancellationToken cancellationToken)
    {
        var departments = await _dbContext.Departments
            .Where(d => d.IsActive == false)
            .Where(d => d.DeletedAt + TimeSpan.FromDays(30) <= DateTime.UtcNow)
            .Include(dl => dl.DepartmentLocations)
            .Include(dp => dp.DepartmentPositions)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Find departments for deletion:{count}", departments.Count);
        return departments;
    }
}