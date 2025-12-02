using System.Runtime.InteropServices.JavaScript;
using Devisions.Application;
using Devisions.Application.Exceptions;
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
        var cleanerTime = _configuration.GetRequiredSection("Cleaner").GetValue<TimeOnly>("RunTime");
        _logger.LogInformation("Cleaner service started. Planned launch: {Time}", cleanerTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = TimeOnly.FromDateTime(DateTime.UtcNow);

            // Вычисляем, сколько ждать до следующего запуска
            TimeSpan delay;
            if (cleanerTime > now)
            {
                // Сегодня ещё не наступило
                delay = cleanerTime.ToTimeSpan() - now.ToTimeSpan();
            }
            else
            {
                // Уже прошло — ждём до завтра
                var timeUntilMidnight = TimeOnly.MaxValue.ToTimeSpan() - now.ToTimeSpan();
                var timeFromMidnight = cleanerTime.ToTimeSpan();
                delay = timeUntilMidnight + timeFromMidnight +
                        TimeSpan.FromSeconds(1); // +1 сек, чтобы не попасть в "сегодня"
            }

            _logger.LogInformation("Next start after {Delay}", delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Cleaner service stopped.");
                return;
            }

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