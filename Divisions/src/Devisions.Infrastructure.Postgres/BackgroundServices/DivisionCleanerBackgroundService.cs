using Devisions.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Devisions.Infrastructure.Postgres.BackgroundServices;

public class DivisionCleanerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DivisionCleanerBackgroundService> _logger;

    public DivisionCleanerBackgroundService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<DivisionCleanerBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cleanerTime = _configuration.GetRequiredSection("Cleaner").GetValue<TimeSpan>("RunTime");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Cleaner service started");
            await Task.Delay(cleanerTime, stoppingToken);
            try
            {
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