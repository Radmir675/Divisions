using Devisions.Application;
using Devisions.Application.Services;
using Devisions.Domain.Department;
using Divisions.IntegrationTests.Infrastructure;
using Divisions.IntegrationTests.Share;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Divisions.IntegrationTests.BackgroundService;

public class CleanBackgroundService : DivisionsBaseTests
{
    public CleanBackgroundService(
        DivisionsTestFactory divisionsTestFactory)
        : base(divisionsTestFactory)
    {
    }
    [Fact]
    public async Task DeleteDepartments_WithRootDepartment_ShouldSucceed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var locationId = await new LocationCreator(Services)
            .CreateAsync("Moscow", cancellationToken);
        await new LocationCreator(Services).SetAsSoftDeletedAsync(locationId, cancellationToken);

        var freeLocationId = await new LocationCreator(Services)
            .CreateAsync("ST-Petersburg", cancellationToken);

        var rootDepartment = Department.CreateParent(
                DepartmentName.Create("root").Value,
                Identifier.Create("root_identifier").Value,
                [locationId])
            .Value;
        rootDepartment.SoftDelete(DateTime.UtcNow.AddDays(-31));
        await new DepartmentCreator(Services).AddAsync(rootDepartment, cancellationToken);

        var firstGenerationDepartment = Department.CreateChild(
                DepartmentName.Create("child").Value,
                Identifier.Create("first_identifier").Value,
                rootDepartment,
                [locationId])
            .Value;
        await new DepartmentCreator(Services).AddAsync(firstGenerationDepartment, cancellationToken);

        var secondGenerationDepartment = Department.CreateChild(
                DepartmentName.Create("sub_child").Value,
                Identifier.Create("second_identifier").Value,
                firstGenerationDepartment,
                [locationId])
            .Value;
        await new DepartmentCreator(Services).AddAsync(secondGenerationDepartment, cancellationToken);

        var positionId = await new PositionCreator(Services).CreateAsync(
            "position",
            [rootDepartment.Id, firstGenerationDepartment.Id],
            cancellationToken);
        await new PositionCreator(Services).SetAsSoftDeletedAsync(positionId, cancellationToken);
        var freePositionId = await new PositionCreator(Services).CreateAsync(
            "position_free",
            [firstGenerationDepartment.Id, secondGenerationDepartment.Id],
            cancellationToken);


        // act
        var processResult = await ExecuteHandler(x => x.Process(cancellationToken));
        var rootDepartmentInDb = await ExecuteInDb(async dbContext =>
        {
            return await dbContext.Departments
                .FirstOrDefaultAsync(x => x.Id == rootDepartment.Id, cancellationToken);
        });
        var firstGenerationDepartmentInDb = await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(c => c.Childrens)
                .FirstOrDefaultAsync(x => x.Id == firstGenerationDepartment.Id, cancellationToken);
            return department;
        });
        var locationsInDb = await ExecuteInDb(async dbContext =>
        {
            var locations = await dbContext.Locations.ToListAsync(cancellationToken);
            return locations;
        });
        var positionsInDb = await ExecuteInDb(async dbContext =>
        {
            var positions = await dbContext.Positions.ToListAsync(cancellationToken);
            return positions;
        });

        // assert
        processResult.IsSuccess.Should().BeTrue();

        rootDepartmentInDb.Should().BeNull();
        firstGenerationDepartmentInDb.Should().NotBeNull();
        firstGenerationDepartmentInDb.ParentId.Should().BeNull();
        firstGenerationDepartmentInDb.Path.PathValue.Should().Be("first_identifier");
        firstGenerationDepartmentInDb.IsActive.Should().BeTrue();
        firstGenerationDepartmentInDb.Childrens.First().Path.PathValue.Should()
            .Be("first_identifier.second_identifier");
        firstGenerationDepartmentInDb.Childrens.First().IsActive.Should().BeTrue();
        firstGenerationDepartmentInDb.Depth.Should().Be(0);
        firstGenerationDepartmentInDb.Childrens.First().Depth.Should().Be(1);

        locationsInDb.Count.Should().Be(1);
        locationsInDb.First().Id.Should().Be(freeLocationId);

        positionsInDb.Count.Should().Be(1);
        positionsInDb.First().Id.Should().Be(freePositionId);
    }

    [Fact]
    public async Task DeleteDepartments_WithNotRootDepartment_ShouldSucceed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var locationId = await new LocationCreator(Services)
            .CreateAsync("Moscow", cancellationToken);
        await new LocationCreator(Services).SetAsSoftDeletedAsync(locationId, cancellationToken);

        var freeLocationId = await new LocationCreator(Services)
            .CreateAsync("ST-Petersburg", cancellationToken);

        var rootDepartment = Department.CreateParent(
                DepartmentName.Create("root").Value,
                Identifier.Create("root_identifier").Value,
                [locationId])
            .Value;
        await new DepartmentCreator(Services).AddAsync(rootDepartment, cancellationToken);

        var firstGenerationDepartment = Department.CreateChild(
                DepartmentName.Create("child").Value,
                Identifier.Create("first_identifier").Value,
                rootDepartment,
                [locationId])
            .Value;
        firstGenerationDepartment.SoftDelete(DateTime.UtcNow.AddDays(-31));
        await new DepartmentCreator(Services).AddAsync(firstGenerationDepartment, cancellationToken);

        var secondGenerationDepartment = Department.CreateChild(
                DepartmentName.Create("sub_child").Value,
                Identifier.Create("second_identifier").Value,
                firstGenerationDepartment,
                [locationId])
            .Value;
        await new DepartmentCreator(Services).AddAsync(secondGenerationDepartment, cancellationToken);

        var positionId = await new PositionCreator(Services).CreateAsync(
            "position",
            [rootDepartment.Id, firstGenerationDepartment.Id],
            cancellationToken);
        await new PositionCreator(Services).SetAsSoftDeletedAsync(positionId, cancellationToken);
        var freePositionId = await new PositionCreator(Services).CreateAsync(
            "position_free",
            [firstGenerationDepartment.Id, secondGenerationDepartment.Id],
            cancellationToken);

        // act
        var processResult = await ExecuteHandler(x => x.Process(cancellationToken));

        var rootDepartmentInDb = await ExecuteInDb(async dbContext =>
        {
            return await dbContext.Departments
                .Include(c => c.Childrens)
                .FirstAsync(x => x.Id == rootDepartment.Id, cancellationToken);
        });
        var firstGenerationDepartmentInDb = await ExecuteInDb(async dbContext =>
        {
            return await dbContext.Departments
                .Include(c => c.Childrens)
                .FirstOrDefaultAsync(x => x.Id == firstGenerationDepartment.Id, cancellationToken);
        });
        var secondGenerationDepartmentInDb = await ExecuteInDb(async dbContext =>
        {
            return await dbContext.Departments
                .Include(c => c.Childrens)
                .FirstAsync(x => x.Id == secondGenerationDepartment.Id, cancellationToken);
        });
        var locationsInDb = await ExecuteInDb(async dbContext =>
        {
            var locations = await dbContext.Locations.ToListAsync(cancellationToken);
            return locations;
        });

        var positionsInDb = await ExecuteInDb(async dbContext =>
        {
            var positions = await dbContext.Positions.ToListAsync(cancellationToken);
            return positions;
        });

        // assert
        processResult.IsSuccess.Should().BeTrue();
        firstGenerationDepartmentInDb.Should().BeNull();

        rootDepartmentInDb.IsActive.Should().BeTrue();
        secondGenerationDepartmentInDb.IsActive.Should().BeTrue();
        rootDepartmentInDb.Childrens.Count.Should().Be(1);
        secondGenerationDepartmentInDb.Path.PathValue.Should().Be("root_identifier.second_identifier");

        locationsInDb.Count.Should().Be(1);
        locationsInDb.First().Id.Should().Be(freeLocationId);

        positionsInDb.Count.Should().Be(1);
        positionsInDb.First().Id.Should().Be(freePositionId);
    }

    [Fact]
    public async Task DeleteDepartments_WithTwoDeletedDepartmentsInOneBranck_ShouldSucceed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var locationId = await new LocationCreator(Services)
            .CreateAsync("Moscow", cancellationToken);
        await new LocationCreator(Services).SetAsSoftDeletedAsync(locationId, cancellationToken);

        var freeLocationId = await new LocationCreator(Services)
            .CreateAsync("ST-Petersburg", cancellationToken);

        var rootDepartment = Department.CreateParent(
                DepartmentName.Create("root").Value,
                Identifier.Create("root_identifier").Value,
                [locationId])
            .Value;
        await new DepartmentCreator(Services).AddAsync(rootDepartment, cancellationToken);

        var firstGenerationDepartment = Department.CreateChild(
                DepartmentName.Create("child").Value,
                Identifier.Create("first_identifier").Value,
                rootDepartment,
                [locationId])
            .Value;
        firstGenerationDepartment.SoftDelete(DateTime.UtcNow.AddDays(-31));
        await new DepartmentCreator(Services).AddAsync(firstGenerationDepartment, cancellationToken);

        var secondGenerationDepartment = Department.CreateChild(
                DepartmentName.Create("sub_child").Value,
                Identifier.Create("second_identifier").Value,
                firstGenerationDepartment,
                [locationId])
            .Value;
        await new DepartmentCreator(Services).AddAsync(secondGenerationDepartment, cancellationToken);

        var thirdGenerationDepartment = Department.CreateChild(
                DepartmentName.Create("sub_child").Value,
                Identifier.Create("third_identifier").Value,
                secondGenerationDepartment,
                [locationId])
            .Value;
        thirdGenerationDepartment.SoftDelete(DateTime.UtcNow.AddDays(-31));
        await new DepartmentCreator(Services).AddAsync(thirdGenerationDepartment, cancellationToken);

        var fourthGenerationDepartment = Department.CreateChild(
                DepartmentName.Create("sub_child").Value,
                Identifier.Create("fourth_identifier").Value,
                thirdGenerationDepartment,
                [locationId])
            .Value;
        await new DepartmentCreator(Services).AddAsync(fourthGenerationDepartment, cancellationToken);

        var positionId = await new PositionCreator(Services).CreateAsync(
            "position",
            [rootDepartment.Id, firstGenerationDepartment.Id],
            cancellationToken);
        await new PositionCreator(Services).SetAsSoftDeletedAsync(positionId, cancellationToken);
        var freePositionId = await new PositionCreator(Services).CreateAsync(
            "position_free",
            [firstGenerationDepartment.Id, secondGenerationDepartment.Id],
            cancellationToken);

        // act
        var processResult = await ExecuteHandler(x => x.Process(cancellationToken));

        var rootDepartmentInDb = await ExecuteInDb(async dbContext =>
        {
            return await dbContext.Departments
                .Include(c => c.Childrens)
                .FirstAsync(x => x.Id == rootDepartment.Id, cancellationToken);
        });
        var firstGenerationDepartmentInDb = await ExecuteInDb(async dbContext =>
        {
            return await dbContext.Departments
                .Include(c => c.Childrens)
                .FirstOrDefaultAsync(x => x.Id == firstGenerationDepartment.Id, cancellationToken);
        });
        var secondGenerationDepartmentInDb = await ExecuteInDb(async dbContext =>
        {
            return await dbContext.Departments
                .Include(c => c.Childrens)
                .FirstAsync(x => x.Id == secondGenerationDepartment.Id, cancellationToken);
        });
        var thirdGenerationDepartmentInDb = await ExecuteInDb(async dbContext =>
        {
            return await dbContext.Departments
                .Include(c => c.Childrens)
                .FirstOrDefaultAsync(x => x.Id == thirdGenerationDepartment.Id, cancellationToken);
        });
        var fourthGenerationDepartmentInDb = await ExecuteInDb(async dbContext =>
        {
            return await dbContext.Departments
                .Include(c => c.Childrens)
                .FirstAsync(x => x.Id == fourthGenerationDepartment.Id, cancellationToken);
        });

        var locationsInDb = await ExecuteInDb(async dbContext =>
        {
            var locations = await dbContext.Locations.ToListAsync(cancellationToken);
            return locations;
        });

        var positionsInDb = await ExecuteInDb(async dbContext =>
        {
            var positions = await dbContext.Positions.ToListAsync(cancellationToken);
            return positions;
        });

        // assert
        processResult.IsSuccess.Should().BeTrue();

        rootDepartmentInDb.Should().NotBeNull();
        firstGenerationDepartmentInDb.Should().BeNull();
        secondGenerationDepartmentInDb.Should().NotBeNull();
        thirdGenerationDepartmentInDb.Should().BeNull();
        fourthGenerationDepartmentInDb.Should().NotBeNull();

        rootDepartmentInDb.Path.PathValue.Should().Be("root_identifier");
        secondGenerationDepartmentInDb.Path.PathValue.Should().Be("root_identifier.second_identifier");
        fourthGenerationDepartmentInDb.Path.PathValue.Should()
            .Be("root_identifier.second_identifier.fourth_identifier");

        rootDepartmentInDb.IsActive.Should().BeTrue();
        secondGenerationDepartmentInDb.IsActive.Should().BeTrue();
        fourthGenerationDepartmentInDb.IsActive.Should().BeTrue();

        rootDepartmentInDb.Depth.Should().Be(0);
        secondGenerationDepartmentInDb.Depth.Should().Be(1);
        fourthGenerationDepartmentInDb.Depth.Should().Be(2);

        rootDepartmentInDb.Childrens.Count.Should().Be(1);
        secondGenerationDepartmentInDb.Childrens.Count.Should().Be(1);

        locationsInDb.Count.Should().Be(1);
        locationsInDb.First().Id.Should().Be(freeLocationId);

        positionsInDb.Count.Should().Be(1);
        positionsInDb.First().Id.Should().Be(freePositionId);
    }

    private async Task<T> ExecuteHandler<T>(Func<IDivisionCleanerService, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<IDivisionCleanerService>();
        return await action(sut);
    }
}