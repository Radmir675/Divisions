using Devisions.Application.Departments.Commands.UpdateLocations;
using Devisions.Contracts.Departments.Requests;
using Devisions.Domain.Department;
using Devisions.Domain.Location;
using Divisions.IntegrationTests.Infrastructure;
using Divisions.IntegrationTests.Share;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Divisions.IntegrationTests.Departments;

public class UpdateLocationsTests : DivisionsBaseTests
{
    public UpdateLocationsTests(DivisionsTestFactory divisionsTestFactory)
        : base(divisionsTestFactory)
    {
    }

    [Fact]
    public async Task UpdateDepartmentLocation_WithValidData_ShouldSucceed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var locationId = await new LocationCreator(Services)
            .CreateAsync("location", cancellationToken);

        var updateLocationsRequest = new UpdateLocationsRequest([locationId.Value]);
        var department = await new DepartmentCreator(Services).CreateAsync(
            null, [locationId], cancellationToken: cancellationToken);

        // act
        var result = await ExecuteHandler(handler =>
        {
            var command = new UpdateLocationsCommand(department.Id.Value, updateLocationsRequest);
            return handler.Handle(command, cancellationToken);
        });

        // assert
        var departmentId = await ExecuteInDb(async dbContext =>
        {
            var departmentId = await dbContext.Departments
                .FirstOrDefaultAsync(x => x.Id == new DepartmentId(result.Value), cancellationToken);
            return departmentId;
        });

        // assert
        Assert.NotNull(departmentId);
        Assert.Equal(result.Value, departmentId.Id.Value);
        Assert.True(result.IsSuccess);
        Assert.NotEqual(result.Value, Guid.Empty);
    }

    [Fact]
    public async Task UpdateDepartmentLocation_WithTwoValidLocations_ShouldSucceed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var locationId1 = await new LocationCreator(Services)
            .CreateAsync("location1", cancellationToken);
        var locationId2 = await new LocationCreator(Services)
            .CreateAsync("location2", cancellationToken);

        var updateLocationsRequest = new UpdateLocationsRequest([locationId1.Value, locationId2.Value]);
        var department =
            await new DepartmentCreator(Services).CreateAsync(null, [locationId1],
                cancellationToken: cancellationToken);

        // act
        var result = await ExecuteHandler(handler =>
        {
            var command = new UpdateLocationsCommand(department.Id.Value, updateLocationsRequest);
            return handler.Handle(command, cancellationToken);
        });

        // assert
        var departmentId = await ExecuteInDb(async dbContext =>
        {
            var departmentId = await dbContext.Departments
                .FirstOrDefaultAsync(x => x.Id == new DepartmentId(result.Value), cancellationToken);
            return departmentId;
        });

        // assert
        Assert.NotNull(departmentId);
        Assert.Equal(result.Value, departmentId.Id.Value);
        Assert.True(result.IsSuccess);
        Assert.NotEqual(result.Value, Guid.Empty);
    }

    [Fact]
    public async Task UpdateDepartmentLocation_WithInvalidLocations_ShouldFail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var locationId1 = await new LocationCreator(Services)
            .CreateAsync("location1", cancellationToken);
        var invalidLocation = new LocationId(Guid.NewGuid());

        var updateLocationsRequest = new UpdateLocationsRequest([locationId1.Value, invalidLocation.Value]);
        var department =
            await new DepartmentCreator(Services).CreateAsync(null, [locationId1],
                cancellationToken: cancellationToken);

        // act
        var result = await ExecuteHandler(handler =>
        {
            var command = new UpdateLocationsCommand(department.Id.Value, updateLocationsRequest);
            return handler.Handle(command, cancellationToken);
        });

        // act
        var departmentId = await ExecuteInDb(async dbContext =>
        {
            var searchId = result.IsSuccess
                ? new DepartmentId(result.Value)
                : new DepartmentId(Guid.NewGuid());

            var departmentId = await dbContext.Departments
                .FirstOrDefaultAsync(x => x.Id == searchId, cancellationToken);
            return departmentId;
        });

        // assert
        Assert.NotEmpty(result.Error);
        Assert.Null(departmentId);
        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<UpdateLocationsHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<UpdateLocationsHandler>();
        return await action(sut);
    }
}