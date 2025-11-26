using Devisions.Domain.Department;
using Devisions.Domain.Position;
using Devisions.Infrastructure.Postgres.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Divisions.IntegrationTests.Share;

public class PositionCreator
{
    private readonly IServiceProvider _services;

    public PositionCreator(IServiceProvider serviceProvider)
    {
        _services = serviceProvider;
    }

    public async Task<PositionId> CreateAsync(string positionName, IEnumerable<DepartmentId> departmentIds,
        CancellationToken cancellationToken)
    {
        await using var scope = _services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var positionId = new PositionId(Guid.NewGuid());

        var departmentPositionsResult = new List<DepartmentPosition>();
        foreach (var departmentId in departmentIds)
        {
            var departmentPositions = new DepartmentPosition(Guid.NewGuid(), departmentId, positionId);
            departmentPositionsResult.Add(departmentPositions);
        }

        var position = Position.Create(
            positionId,
            PositionName.Create(positionName).Value,
            null,
            departmentPositionsResult);


        dbContext.Positions.Add(position.Value);
        await dbContext.SaveChangesAsync(cancellationToken);


        return positionId;
    }
}