using CSharpFunctionalExtensions;
using Devisions.Application.Positions;
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
}