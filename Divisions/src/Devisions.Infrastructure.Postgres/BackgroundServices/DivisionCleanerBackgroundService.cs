using Devisions.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Devisions.Infrastructure.Postgres.BackgroundServices;

public class DivisionCleanerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DivisionCleanerBackgroundService> _logger;

    public DivisionCleanerBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<DivisionCleanerBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var targetTime = new TimeSpan(0, 10, 10);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(targetTime, stoppingToken);
                await using var scope = _scopeFactory.CreateAsyncScope();
                var divisionCleanerService = scope.ServiceProvider.GetRequiredService<IDivisionCleanerService>();
                var result = await divisionCleanerService.Process(stoppingToken);
                if (result.IsFailure)
                    _logger.LogError("Error processing division cleaner");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in division cleaner");
            }
        }
    }
}