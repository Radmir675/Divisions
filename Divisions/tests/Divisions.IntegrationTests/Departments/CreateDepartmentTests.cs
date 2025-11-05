using Devisions.Application.Departments.CreateDepartment;
using Devisions.Contracts.Departments;
using Devisions.Domain.Location;
using Devisions.Infrastructure.Postgres.Database;
using Divisions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Divisions.IntegrationTests.Departments;

public class CreateDepartmentTests : IClassFixture<DivisionsTestFactory>, IAsyncLifetime
{
    private readonly IServiceProvider _services;
    private readonly Func<Task> _resetDatabase;

    public CreateDepartmentTests(DivisionsTestFactory divisionsTestFactory)
    {
        _services = divisionsTestFactory.Services;
        _resetDatabase = divisionsTestFactory.ResetDatabaseAsync;
    }

    [Fact]
    public async Task CreateDepartment_with_valid_data_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationAsync(cancellationToken);

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

        // assert
        var department = await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(x => x.Id.Value == result.Value, cancellationToken);
            return department;
        });

        Assert.NotNull(department);
        Assert.Equal(result.Value, department.Id.Value);
        Assert.True(result.IsSuccess);
        Assert.NotEqual(result.Value, Guid.Empty);
    }

    private async Task<LocationId> CreateLocationAsync(CancellationToken cancellationToken)
    {
        var result = await ExecuteInDb(async dbContext =>
        {
            var location = Location.Create(
                "newLocation",
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
        await using var scope = _services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();
        return await action(sut);
    }

    private async Task<T> ExecuteInDb<T>(Func<AppDbContext, Task<T>> action)
    {
        await using var scope = _services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await action(dbContext);
    }

    public async Task InitializeAsync() => await Task.CompletedTask;

    public async Task DisposeAsync() => await _resetDatabase();
}