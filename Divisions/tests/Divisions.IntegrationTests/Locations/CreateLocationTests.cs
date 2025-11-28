using Devisions.Application.Locations.Commands.Create;
using Devisions.Contracts.Locations.Requests;
using Devisions.Domain.Location;
using Divisions.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Errors;

namespace Divisions.IntegrationTests.Locations;

public class CreateLocationTests : DivisionsBaseTests
{
    public CreateLocationTests(DivisionsTestFactory divisionsTestFactory)
        : base(divisionsTestFactory)
    {
    }

    [Fact]
    public async Task CreateLocation_WithValidData_ShouldSucceed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var locationIdResult = await ExecuteHandler((sut) =>
        {
            var request = new CreateLocationRequest(
                "Sankt-Petersburg",
                new AddressRequest("Russia", "Sankt-Petersburg", "lenina", 1, null),
                "Europe/Moscow");
            var command = new CreateLocationCommand(request);
            return sut.Handle(command, cancellationToken);
        });

        // act
        var location = await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Locations
                .FirstAsync(x => x.Id == new LocationId(locationIdResult.Value), cancellationToken);
            return department;
        });

        // assert
        locationIdResult.IsSuccess.Should().BeTrue();
        locationIdResult.Value.Should().NotBe(Guid.Empty);
        location.Id.Should().Be(new LocationId(locationIdResult.Value));
    }

    [Fact]
    public async Task CreateLocation_WithInvalidName_ShouldFail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var locationValidResult = await ExecuteHandler((sut) =>
        {
            var request = new CreateLocationRequest(
                "Sankt-Petersburg",
                new AddressRequest("Russia", "Sankt-Petersburg", "lenina", 1, null),
                "Europe/Moscow");
            var command = new CreateLocationCommand(request);
            return sut.Handle(command, cancellationToken);
        });

        var locationInvalidResult = await ExecuteHandler((sut) =>
        {
            var request = new CreateLocationRequest(
                "Sankt-Petersburg",
                new AddressRequest("Rome", "Greece", "Sparta", 1, null),
                "Europe/Moscow");
            var command = new CreateLocationCommand(request);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        locationValidResult.IsSuccess.Should().BeTrue();
        locationInvalidResult.IsSuccess.Should().BeFalse();
        locationInvalidResult.Error.Should().NotBeNull();
        locationInvalidResult.Error.First().ErrorType.Should().Be(ErrorType.VALIDATION);
        locationInvalidResult.Error.First().Code.Should().Be("record.already.exist");
    }

    private async Task<T> ExecuteHandler<T>(Func<CreateLocationHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<CreateLocationHandler>();
        return await action(sut);
    }
}