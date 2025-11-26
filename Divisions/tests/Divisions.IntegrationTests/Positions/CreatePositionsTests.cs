using Devisions.Application.Positions.Create;
using Devisions.Contracts.Positions.Requests;
using Devisions.Domain.Location;
using Devisions.Domain.Position;
using Divisions.IntegrationTests.Infrastructure;
using Divisions.IntegrationTests.Share;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Divisions.IntegrationTests.Positions;

public class CreatePositionsTests : DivisionsBaseTests
{
    public CreatePositionsTests(DivisionsTestFactory divisionsTestFactory)
        : base(divisionsTestFactory)
    {
    }

    [Fact]
    public async Task CreatePosition_WithValidData_ShouldSucceed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var location = await new LocationCreator(Services)
            .CreateAsync("location", cancellationToken);

        var department = await new DepartmentCreator(Services).CreateAsync(
            null,
            [new LocationId(location.Value)],
            cancellationToken: cancellationToken);

        var positionIdResult = await ExecuteHandler((sut) =>
        {
            var request = new CreatePositionRequest(
                "Position",
                null,
                [department.Id.Value]);

            var command = new CreatePositionCommand(request);
            return sut.Handle(command, cancellationToken);
        });

        // act
        var position = await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstAsync(x => x.Id == new PositionId(positionIdResult.Value), cancellationToken);
            return position;
        });

        // assert
        positionIdResult.IsSuccess.Should().BeTrue();
        positionIdResult.Value.Should().NotBe(Guid.Empty);
        position.Id.Should().Be(new PositionId(positionIdResult.Value));
    }

    [Fact]
    public async Task CreatePosition_WithInvalidRandomDepartments_ShouldFail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var randomDepartmentId = Guid.NewGuid();
        var positionIdResult = await ExecuteHandler((sut) =>
        {
            var request = new CreatePositionRequest(
                "Position",
                null,
                [randomDepartmentId]);

            var command = new CreatePositionCommand(request);
            return sut.Handle(command, cancellationToken);
        });

        // act
        var position = await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Positions
                .FirstOrDefaultAsync(x => x.Id == new PositionId(randomDepartmentId), cancellationToken);
            return department;
        });

        // assert
        positionIdResult.IsFailure.Should().BeTrue();
        position.Should().BeNull();
    }

    private async Task<T> ExecuteHandler<T>(Func<CreatePositionsHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<CreatePositionsHandler>();
        return await action(sut);
    }
}