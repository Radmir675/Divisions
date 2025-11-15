using Devisions.Infrastructure.Postgres.Database;
using Microsoft.Extensions.Logging;

namespace Devisions.Infrastructure.Postgres.Seeder;

public class DevisionsSeeder : ISeeder
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DevisionsSeeder> _logger;
    private readonly CancellationToken _cancellationToken;

    public DevisionsSeeder(
        AppDbContext dbContext,
        ILogger<DevisionsSeeder> logger,
        CancellationToken cancellationToken = default)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cancellationToken = cancellationToken;
    }

    public async Task CreateAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SeedAsync(_cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while seeding the database");
        }
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Seeding started");
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(_cancellationToken);

        await SeedDepartmentsAsync(cancellationToken);
        await SeedLocationsAsync(cancellationToken);
        await transaction.CommitAsync(_cancellationToken);

        _logger.LogInformation("Seeding completed");
    }

    private async Task SeedLocationsAsync(CancellationToken cancellationToken)
    {
        // TODO: дописать сидировани
    }

    private async Task SeedDepartmentsAsync(CancellationToken cancellationToken)
    {
        // TODO: дописать сидирование
    }
}