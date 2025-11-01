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

    public async Task<Result<IEnumerable<DepartmentId>, Error>> LockDescendants(
        Path rootPath,
        CancellationToken cancellationToken)
    {
        // TODO: нужну заблочить только дочерние
        try
        {
            const string query = """
                                 SELECT *
                                 FROM departments
                                 WHERE path <@ @rootPath::ltree
                                 ORDER BY depth
                                 FOR UPDATE
                                 """;

            await using var connection = _dbContext.Database.GetDbConnection();
            var departmentIds = (await connection
                    .QueryAsync<Department>(query, new { rootPath.PathValue }))
                .Select(x => x.Id)
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
            const string query = """
                                  SELECT * 
                                  FROM departments
                                  WHERE id=@departmentId
                                  FOR UPDATE
                                 """;
            await using var connection = _dbContext.Database.GetDbConnection();
            var department = await connection.QueryFirstAsync<Department>(query, new { departmentId });
            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to lock by id");
            return Error.Failure("fail.lock", "Failed to lock by id");
        }
    }

    public async Task<UnitResult<Error>> UpdatePathDescendants(Path oldPath, Path newPath,
        CancellationToken cancellationToken)
    {
        try
        {
            string query = @"UPDATE departments
                           SET path= @newPath::ltree || subpath(path, nlevel(@oldPath::ltree))
                           WHERE path <@ @basePath::ltree";

            await _dbContext.Database.ExecuteSqlRawAsync(
                query,
                new NpgsqlParameter("oldPath", oldPath.PathValue),
                new NpgsqlParameter("newPath", newPath.PathValue));

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
            string query = @"UPDATE departments
                         SET depth =depth + @depth
                         WHERE path <@ @basePath::ltree";
            await _dbContext.Database.ExecuteSqlRawAsync(
                query,
                new NpgsqlParameter("basePath", path.PathValue), cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update path");
            return UnitResult.Failure(Error.Failure("fail.update.depth.", "Failed to update depth"));
        }
    }
}