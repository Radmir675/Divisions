using Devisions.Application.Departments.Commands.Move;
using Devisions.Domain.Department;
using Divisions.IntegrationTests.Infrastructure;
using Divisions.IntegrationTests.Share;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Errors;

namespace Divisions.IntegrationTests.Departments;

public class MoveDepartmentsTests : DivisionsBaseTests
{
    public MoveDepartmentsTests(DivisionsTestFactory divisionsTestFactory)
        : base(divisionsTestFactory)
    {
        Init().GetAwaiter().GetResult();
    }

    private Department _root;
    private Department _underRoot;
    private Department _underRootChildren;
    private Department _children;

    private async Task Init()
    {
        var cancellationToken = CancellationToken.None;

        var locationId = await new LocationCreator(Services).CreateAsync(
            "location",
            cancellationToken);

        _root = await new DepartmentCreator(Services)
            .CreateAsync(null, [locationId], "root", cancellationToken);

        _underRoot = await new DepartmentCreator(Services)
            .CreateAsync(_root, [locationId], "under_root", cancellationToken);

        _underRootChildren = await new DepartmentCreator(Services)
            .CreateAsync(_underRoot, [locationId], "under_root_children", cancellationToken);

        _children = await new DepartmentCreator(Services)
            .CreateAsync(_underRootChildren, [locationId], "children", cancellationToken);
    }

    [Fact]
    public async Task MoveDepartment_ToParentNull_ShouldSucceed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        const string firstGenerationPath = "under_root";
        const string secondGenerationPath = "under_root.under_root_children";
        const string thirdGenerationPath = "under_root.under_root_children.children";

        const int firstGenerationDepth = 0;
        const int secondGenerationDepth = 1;
        const int thirdGenerationDepth = 2;

        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new MoveDepartmentCommand(_underRoot.Id.Value, null);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        var department = await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(x => x.Childrens)
                .ThenInclude(c => c.Childrens)
                .FirstAsync(x => x.Id == new DepartmentId(result.Value), cancellationToken);
            return department;
        });

        var firstGenerationChildren = department.Childrens.First();
        var secondGenerationChildren = department.Childrens.First().Childrens.First();

        department.Should().NotBeNull();
        result.Value.Should().Be(department.Id.Value);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        department.Path.PathValue.Should().Be(firstGenerationPath);
        department.Depth.Should().Be(firstGenerationDepth);

        firstGenerationChildren.Path.PathValue.Should().Be(secondGenerationPath);
        firstGenerationChildren.Depth.Should().Be(secondGenerationDepth);

        secondGenerationChildren.Path.PathValue.Should().Be(thirdGenerationPath);
        secondGenerationChildren.Depth.Should().Be(thirdGenerationDepth);
    }

    [Fact]
    public async Task MoveDepartment_ToParent_ShouldSucceed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        const string firstGenerationPath = "root.under_root_children";
        const string secondGenerationPath = "root.under_root_children.children";

        const int firstGenerationDepth = 1;
        const int secondGenerationDepth = 2;

        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new MoveDepartmentCommand(_underRootChildren.Id.Value, _root.Id.Value);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        var department = await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(x => x.Childrens)
                .FirstAsync(x => x.Id == new DepartmentId(result.Value), cancellationToken);
            return department;
        });

        var firstGenerationChildren = department.Childrens.First();

        department.Should().NotBeNull();
        result.Value.Should().Be(department.Id.Value);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        department.Path.PathValue.Should().Be(firstGenerationPath);
        department.Depth.Should().Be(firstGenerationDepth);

        firstGenerationChildren.Path.PathValue.Should().Be(secondGenerationPath);
        firstGenerationChildren.Depth.Should().Be(secondGenerationDepth);
    }

    [Fact]
    public async Task MoveDepartment_ToItsChild_ShouldFail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new MoveDepartmentCommand(_underRoot.Id.Value, _underRootChildren.Id.Value);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.First().ErrorType.Should().Be(ErrorType.CONFLICT);
        result.Error.First().Code.Should().Be("parent.id.failure");
    }


    [Fact]
    public async Task MoveDepartment_WithSameParentAndDepartmentId_ShouldFail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new MoveDepartmentCommand(_underRoot.Id.Value, _underRoot.Id.Value);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.First().ErrorType.Should().Be(ErrorType.VALIDATION);
        result.Error.First().Code.Should().Be("input.failure");
    }

    private async Task<T> ExecuteHandler<T>(Func<MoveDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<MoveDepartmentHandler>();
        return await action(sut);
    }
}