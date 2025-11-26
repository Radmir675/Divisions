using Devisions.Application.Departments.Commands.SoftDelete;
using Devisions.Domain.Department;
using Devisions.Infrastructure.Postgres.Database;
using Divisions.IntegrationTests.Infrastructure;
using Divisions.IntegrationTests.Share;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Divisions.IntegrationTests.Departments;

public class SoftDeleteDepartmentTest : DivisionsBaseTests
{
    public SoftDeleteDepartmentTest(DivisionsTestFactory divisionsTestFactory)
        : base(divisionsTestFactory)
    {
    }

    [Fact]
    public async Task SoftDeleteDepartment_WithValidData_ShouldSucceed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // локации
        var mainLocation = await new LocationCreator(Services)
            .CreateAsync("main_location", cancellationToken);

        var subLocation = await new LocationCreator(Services)
            .CreateAsync("sub_location", cancellationToken);

        var subSubLocation = await new LocationCreator(Services)
            .CreateAsync("sub_sub_location", cancellationToken);

        // департаменты
        var firstDepartment = await new DepartmentCreator(Services).CreateAsync(
            null, [mainLocation, subLocation], "firstDepartment", cancellationToken);

        var secondDepartment = await new DepartmentCreator(Services).CreateAsync(
            firstDepartment, [mainLocation, subLocation], "secondDepartment", cancellationToken);

        var thirdDepartment = await new DepartmentCreator(Services).CreateAsync(
            secondDepartment, [mainLocation, subLocation], "thirdDepartment", cancellationToken);
        var fourthDepartment = await new DepartmentCreator(Services).CreateAsync(
            secondDepartment, [subLocation, subSubLocation], "fourthDepartment", cancellationToken);

        // позиции
        var secondPosition =
            await new PositionCreator(Services).CreateAsync("position2", [secondDepartment.Id], cancellationToken);
        var thirdPosition =
            await new PositionCreator(Services).CreateAsync("position3", [secondDepartment.Id], cancellationToken);
        var fourthPosition =
            await new PositionCreator(Services).CreateAsync("position4", [fourthDepartment.Id], cancellationToken);

        // удаляем второй департамент
        // у которого все локации должны быть активны так как используются в других подразделениях 
        // и позиции  не активны
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(secondDepartment.Id.Value);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        var department = await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(dp => dp.DepartmentPositions)
                .Include(dl => dl.DepartmentLocations)
                .Include(x => x.Childrens)
                .FirstAsync(x => x.Id == new DepartmentId(result.Value), cancellationToken);
            return department;
        });

        string secondDepartmentPath = "firstDepartment.deleted_secondDepartment";
        string thirdDepartmentPath = "firstDepartment.deleted_secondDepartment.thirdDepartment";
        string fourthDepartmentPath = "firstDepartment.deleted_secondDepartment.fourthDepartment";

        department.Should().NotBeNull();
        result.Value.Should().Be(department.Id.Value);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        // пути
        department.Path.PathValue.Should().Be(secondDepartmentPath);
        department.Childrens.First(c => c.Identifier.Identify == "thirdDepartment").Path.PathValue
            .Should().Be(thirdDepartmentPath);
        department.Childrens.First(c => c.Identifier.Identify == "fourthDepartment").Path.PathValue
            .Should().Be(fourthDepartmentPath);


        // активности
        department.IsActive.Should().BeFalse();

        // проверить позиции

        var secondDepartmentPositionsInDb = await ExecuteInDb(async dbContext =>
        {
            var positionIds = await dbContext.DepartmentPositions
                .Where(x => x.DepartmentId == secondDepartment.Id)
                .Select(x => x.PositionId)
                .ToListAsync(cancellationToken);

            var positions = await dbContext.Positions
                .Where(p => positionIds.Contains(p.Id))
                .ToListAsync(cancellationToken);
            return positions;
        });

        secondDepartmentPositionsInDb.All(x => x.IsActive).Should().BeFalse();

        // проверить локации

        var secondDepartmentLocationsInDb = await ExecuteInDb(async dbContext =>
        {
            var locationIds = await dbContext.DepartmentLocations
                .Where(x => x.DepartmentId == secondDepartment.Id)
                .Select(x => x.LocationId)
                .ToListAsync(cancellationToken);
            var locations = await dbContext.Locations
                .Where(z => locationIds.Contains(z.Id))
                .ToListAsync(cancellationToken);
            return locations;
        });

        secondDepartmentLocationsInDb.All(x => x.IsActive).Should().BeTrue();
    }


    [Fact]
    public async Task SoftDeleteDepartment_InValidData_ShouldFailed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // локации
        var mainLocation = await new LocationCreator(Services)
            .CreateAsync("main_location", cancellationToken);

        var subLocation = await new LocationCreator(Services)
            .CreateAsync("sub_location", cancellationToken);

        var subSubLocation = await new LocationCreator(Services)
            .CreateAsync("sub_sub_location", cancellationToken);

        // департаменты
        var firstDepartment = await new DepartmentCreator(Services).CreateAsync(
            null, [mainLocation, subLocation], "firstDepartment", cancellationToken);

        var secondDepartment = await new DepartmentCreator(Services).CreateAsync(
            firstDepartment, [subSubLocation, subLocation], "secondDepartment", cancellationToken);

        var thirdDepartment = await new DepartmentCreator(Services).CreateAsync(
            secondDepartment, [subLocation], "thirdDepartment", cancellationToken);
        var fourthDepartment = await new DepartmentCreator(Services).CreateAsync(
            secondDepartment, [subLocation, subSubLocation], "fourthDepartment", cancellationToken);

        // позиции
        var secondPosition =
            await new PositionCreator(Services).CreateAsync(
                "position2",
                [firstDepartment.Id, secondDepartment.Id],
                cancellationToken);
        var thirdPosition =
            await new PositionCreator(Services).CreateAsync("position3", [firstDepartment.Id], cancellationToken);

        // удаляем первый департамент

        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteDepartmentCommand(firstDepartment.Id.Value);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        var department = await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(dp => dp.DepartmentPositions)
                .Include(dl => dl.DepartmentLocations)
                .Include(x => x.Childrens)
                .FirstAsync(x => x.Id == new DepartmentId(result.Value), cancellationToken);
            return department;
        });

        string firstDepartmentPath = "deleted_firstDepartment";
        string secondDepartmentPath = "deleted_firstDepartment.secondDepartment";
        string thirdDepartmentPath = "deleted_firstDepartment.secondDepartment.thirdDepartment";
        string fourthDepartmentPath = "deleted_firstDepartment.secondDepartment.fourthDepartment";

        department.Should().NotBeNull();
        result.Value.Should().Be(department.Id.Value);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        // пути
        department.Path.PathValue.Should().Be(firstDepartmentPath);
        department.Childrens.First(c => c.Identifier.Identify == "secondDepartment").Path.PathValue
            .Should().Be(secondDepartmentPath);
        var thirdGenerationChildrens = await ExecuteInDb(async dbContext =>
        {
            var parentId = secondDepartment.Id;
            var childrens = await dbContext.Departments
                .Where(d => d.ParentId == parentId)
                .ToListAsync(cancellationToken);
            return childrens;
        });
        thirdGenerationChildrens.First(x => x.Identifier.Identify == "thirdDepartment")
            .Path.PathValue.Should()
            .Be(thirdDepartmentPath);
        thirdGenerationChildrens.First(x => x.Identifier.Identify == "fourthDepartment")
            .Path.PathValue.Should()
            .Be(fourthDepartmentPath);

        // активности
        department.IsActive.Should().BeFalse();

        // проверить позиции

        var firstDepartmentPositionsInDb = await ExecuteInDb(async dbContext =>
        {
            var positionIds = await dbContext.DepartmentPositions
                .Where(x => x.DepartmentId == firstDepartment.Id)
                .Select(x => x.PositionId)
                .ToListAsync(cancellationToken);

            var positions = await dbContext.Positions
                .Where(p => positionIds.Contains(p.Id))
                .ToListAsync(cancellationToken);
            return positions;
        });

        firstDepartmentPositionsInDb.First(x => x.Id == secondPosition).IsActive.Should().BeTrue();
        firstDepartmentPositionsInDb.First(x => x.Id == thirdPosition).IsActive.Should().BeFalse();

        // проверить локации

        var firstDepartmentLocationsInDb = await ExecuteInDb(async dbContext =>
        {
            var locationIds = await dbContext.DepartmentLocations
                .Where(x => x.DepartmentId == firstDepartment.Id)
                .Select(x => x.LocationId)
                .ToListAsync(cancellationToken);
            var locations = await dbContext.Locations
                .Where(z => locationIds.Contains(z.Id))
                .ToListAsync(cancellationToken);
            return locations;
        });
        firstDepartmentLocationsInDb.First(x => x.Id == mainLocation).IsActive.Should().BeFalse();
        firstDepartmentLocationsInDb.First(x => x.Id == subLocation).IsActive.Should().BeTrue();
    }

    private async Task<T> ExecuteHandler<T>(Func<SoftDeleteDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<SoftDeleteDepartmentHandler>();
        return await action(sut);
    }

    private async Task<AppDbContext> GetDbContext()
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return sut;
    }
}