using Devisions.Infrastructure.Postgres.Database;
using Divisions.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Divisions.IntegrationTests;

public class DivisionsBaseTests : IClassFixture<DivisionsTestFactory>, IAsyncLifetime
{
    protected IServiceProvider Services { get; set; }

    private readonly Func<Task> _resetDatabase;

    protected DivisionsBaseTests(DivisionsTestFactory divisionsTestFactory)
    {
        Services = divisionsTestFactory.Services;
        _resetDatabase = divisionsTestFactory.ResetDatabaseAsync;
    }

    public async Task InitializeAsync() => await Task.CompletedTask;

    public async Task DisposeAsync() => await _resetDatabase();

    protected async Task<T> ExecuteInDb<T>(Func<AppDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await action(dbContext);
    }
}