using Devisions.Application.Departments.Commands.Create;
using Devisions.Contracts.Departments.Requests;
using Devisions.Domain.Department;
using Devisions.Domain.Location;
using Divisions.IntegrationTests.Infrastructure;
using Divisions.IntegrationTests.Share;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Divisions.IntegrationTests.Departments;

public class CreateDepartmentTests : DivisionsBaseTests
{
    public CreateDepartmentTests(DivisionsTestFactory divisionsTestFactory)
        : base(divisionsTestFactory)
    {
    }

    [Fact]
    public async Task CreateDepartment_with_valid_data_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var locationId = await new LocationCreator(Services)
            .CreateAsync(
                "location",
                cancellationToken);

        var result = await ExecuteHandler((sut) =>
        {
            var request = new CreateDepartmentRequest(
                "test",
                "test_identifier",
                null,
                [locationId.Value]);
            var command = new CreateDepartmentCommand(request);
            return sut.Handle(command, cancellationToken);
        });

        // act
        var department = await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(x => x.Id == new DepartmentId(result.Value), cancellationToken);
            return department;
        });

        // assert
        Assert.NotNull(department);
        Assert.Equal(result.Value, department.Id.Value);
        Assert.True(result.IsSuccess);
        Assert.NotEqual(result.Value, Guid.Empty);
    }

    [Fact]
    public async Task CreateDepartment_with_multiple_locations_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var firstLocationId = await CreateLocationAsync("first_location", cancellationToken);
        var secondLocationId = await CreateLocationAsync("second_location", cancellationToken);

        var departmentResult = await ExecuteHandler((sut) =>
        {
            var request = new CreateDepartmentRequest(
                "test",
                "test_identifier",
                null,
                [firstLocationId.Value, secondLocationId.Value]);
            var command = new CreateDepartmentCommand(request);
            return sut.Handle(command, cancellationToken);
        });

        // act
        var department = await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(x => x.Id == new DepartmentId(departmentResult.Value), cancellationToken);
            return department;
        });

        // assert
        Assert.NotNull(department);
        Assert.Equal(departmentResult.Value, department.Id.Value);
        Assert.True(departmentResult.IsSuccess);
        Assert.NotEqual(departmentResult.Value, Guid.Empty);
    }

    [Fact]
    public async Task CreateDepartment_with_invalid_location_should_failure()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var firstLocationId = await CreateLocationAsync("first_location", cancellationToken);
        var invalidLocation = new LocationId(Guid.NewGuid());

        var departmentResult = await ExecuteHandler((sut) =>
        {
            var request = new CreateDepartmentRequest(
                "test",
                "test_identifier",
                null,
                [firstLocationId.Value, invalidLocation.Value]);
            var command = new CreateDepartmentCommand(request);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(departmentResult.IsFailure);
        Assert.NotEmpty(departmentResult.Error);
    }

    [Fact]
    public async Task CreateDepartment_without_locations_should_failure()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var departmentResult = await ExecuteHandler((sut) =>
        {
            var request = new CreateDepartmentRequest(
                "test",
                "test_identifier",
                null,
                [Guid.NewGuid()]);
            var command = new CreateDepartmentCommand(request);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(departmentResult.IsFailure);
        Assert.NotEmpty(departmentResult.Error);
    }

    [Fact]
    public async Task CreateDepartments_with_identical_identifiers_should_failure()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var locationId = await new LocationCreator(Services).CreateAsync("location", cancellationToken);

        var departmentResult = await ExecuteHandler((sut) =>
        {
            var request = new CreateDepartmentRequest(
                "test",
                "test_identifier",
                null,
                [locationId.Value]);
            var command = new CreateDepartmentCommand(request);
            return sut.Handle(command, cancellationToken);
        });
        var departmentResultFailure = await ExecuteHandler((sut) =>
        {
            var request = new CreateDepartmentRequest(
                "test",
                "test_identifier",
                null,
                [locationId.Value]);
            var command = new CreateDepartmentCommand(request);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(departmentResultFailure.IsFailure);
        Assert.NotEmpty(departmentResultFailure.Error);
    }

    private async Task<LocationId> CreateLocationAsync(string locationName, CancellationToken cancellationToken)
    {
        var result = await ExecuteInDb(async dbContext =>
        {
            var location = Location.Create(
                locationName,
                Address.Create("Russia", "Moscow", "Lenina", 1, null).Value,
                true,
                Timezone.Create("Europe/Moscow").Value);

            dbContext.Locations.Add(location.Value);
            await dbContext.SaveChangesAsync(cancellationToken);
            var locationId = location.Value.Id;

            return locationId;
        });
        return result;
    }

    private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();
        return await action(sut);
    }
}