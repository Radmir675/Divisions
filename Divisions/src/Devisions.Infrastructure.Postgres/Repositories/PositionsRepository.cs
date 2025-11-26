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

    public async Task<Result<bool, Error>> IsNameActiveAndFreeAsync(
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
                .ToListAsync(cancellationToken);

            if (positions.Count != positionIds.Count())
            {
                return GeneralErrors.NotFoundInDatabase();
            }

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

    public async Task<Result<IEnumerable<PositionId>, Error>> GetUnusedAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            // const string sql = """
            //                    SELECT *
            //                    FROM department_positions
            //                    WHERE department_id != 'f4469485-920b-45b1-a783-9c4518600604'
            //                      AND position_id IN (SELECT position_id
            //                                          FROM department_positions
            //                                          WHERE department_id = 'f4469485-920b-45b1-a783-9c4518600604')
            //                    """;
            //
            // object[] parameters = { new NpgsqlParameter("DepartmentId", departmentId.Value) };
            // var result = await _dbContext.DepartmentPositions
            //     .FromSqlRaw("SELECT * FROM department_positions")
            //     .ToListAsync(cancellationToken);


            // var positionIds = result.Select(x => x.PositionId).ToList();


            var result = await _dbContext.DepartmentPositions
                .FromSqlInterpolated($@"
                                                SELECT dp.*
                                                FROM department_positions dp
                                                WHERE dp.department_id = {departmentId.Value}
                                            ")
                .ToListAsync(cancellationToken);

            Console.WriteLine($"Найдено: {result.Count}");


            var positionIds = result.Select(x => x.PositionId).ToList();
            return positionIds;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Result<IEnumerable<Position>, Error>> GetByIds(
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
}