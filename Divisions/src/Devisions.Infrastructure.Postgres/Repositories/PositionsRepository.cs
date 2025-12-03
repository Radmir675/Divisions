using CSharpFunctionalExtensions;
using Devisions.Application.Positions;
using Devisions.Domain.Department;
using Devisions.Domain.Position;
using Devisions.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Infrastructure.Postgres.Repositories;

public class PositionsRepository : IPositionsRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<PositionsRepository> _logger;

    public PositionsRepository(AppDbContext dbContext, ILogger<PositionsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Positions.Add(position);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Error.Failure(
                "positionsRepository.AddAsync",
                "Position could not be added in repository");
        }

        return position.Id.Value;
    }

    public async Task<Result<bool, Error>> IsNameAvailableAsync(
        PositionName name,
        CancellationToken cancellationToken)
    {
        var existActiveName = await _dbContext.Positions
            .AsNoTracking()
            .AnyAsync(
                p => p.Name == name && p.IsActive,
                cancellationToken);

        return !existActiveName;
    }

    public async Task<UnitResult<Error>> DeleteAsync(
        IEnumerable<PositionId> positionIds,
        CancellationToken cancellationToken)
    {
        try
        {
            var positions = await _dbContext.Positions
                .Where(p => positionIds.Contains(p.Id))
                .Include(position => position.DepartmentPositions)
                .ToListAsync(cancellationToken);

            if (positions.Count != positionIds.Count())
            {
                return GeneralErrors.NotFoundInDatabase();
            }

            _dbContext.DepartmentPositions.RemoveRange(positions.SelectMany(x => x.DepartmentPositions));
            _dbContext.Positions.RemoveRange(positions);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, "Error deleting positions");
            return GeneralErrors.DatabaseError();
        }
    }

    public async Task<Result<IEnumerable<PositionId>, Error>> GetPositionsExclusiveToAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT id, department_id, position_id
                           FROM department_positions
                           WHERE department_id = {0}
                             AND position_id NOT IN (SELECT position_id
                                                 FROM department_positions
                                                 WHERE department_id != {0})
                           """;

        var result = await _dbContext.DepartmentPositions
            .FromSqlRaw(sql, departmentId.Value)
            .ToListAsync(cancellationToken);

        var positionIds = result.Select(x => x.PositionId).ToList();

        _logger.LogDebug("Founded: {positionsCount} positions", result.Count);

        return positionIds;
    }

    public async Task<Result<IEnumerable<Position>, Error>> GetByIdsAsync(
        IEnumerable<PositionId> positionIds,
        CancellationToken cancellationToken)
    {
        var result = await _dbContext.Positions
            .Include(x => x.DepartmentPositions)
            .Where(x => positionIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (result.Count == 0)
            return GeneralErrors.NotFoundInDatabase();

        return result;
    }

    public async Task<IEnumerable<Position>> GetRemovableAsync(
        CancellationToken cancellationToken)
    {
        var positions = await _dbContext.Positions
            .Where(d => d.IsActive == false)
            .Where(d => d.DeletedAt + TimeSpan.FromDays(30) <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Find position for deletion:{count}", positions.Count);
        return positions;
    }
}