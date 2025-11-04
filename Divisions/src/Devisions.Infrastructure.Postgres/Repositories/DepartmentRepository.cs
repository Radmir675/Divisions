using CSharpFunctionalExtensions;
using Dapper;
using Devisions.Application.Database;
using Devisions.Application.Departments;
using Devisions.Domain.Department;
using Devisions.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Path = Devisions.Domain.Department.Path;

namespace Devisions.Infrastructure.Postgres.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _dbContext;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<LocationRepository> _logger;

    public DepartmentRepository(
        AppDbContext dbContext,
        IDbConnectionFactory dbConnectionFactory,
        ILogger<LocationRepository> logger)
    {
        _dbContext = dbContext;
        _dbConnectionFactory = dbConnectionFactory;
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
            x => x.Identifier == identifier,
            cancellationToken);
        return result;
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

    public async Task<UnitResult<Error>> UpdatePathDescendants(
        Path oldPath,
        Path newPath,
        CancellationToken cancellationToken)
    {
        try
        {
            FormattableString query = $@"UPDATE departments
                           SET path= {newPath.PathValue}::ltree || subpath(path, nlevel({oldPath.PathValue}::ltree))
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

    public async Task<UnitResult<Error>> UpdateDepthDescendants(
        Path path,
        int deltaDepth,
        CancellationToken cancellationToken)
    {
        try
        {
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
}