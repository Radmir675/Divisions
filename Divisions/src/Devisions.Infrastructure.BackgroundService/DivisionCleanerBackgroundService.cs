using Devisions.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Devisions.Infrastructure.BackgroundService;

public class DivisionCleanerBackgroundService : Microsoft.Extensions.Hosting.BackgroundService
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
        var targetTime = new TimeSpan(0, 0, 10);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(targetTime, stoppingToken);
                await using var scope = _scopeFactory.CreateAsyncScope();
                var s = scope.ServiceProvider.GetRequiredService<IDivisionCleanerService>();
                await s.Process(stoppingToken);
            }
            catch (Exception ex)
            {
            }
        }
    }
}