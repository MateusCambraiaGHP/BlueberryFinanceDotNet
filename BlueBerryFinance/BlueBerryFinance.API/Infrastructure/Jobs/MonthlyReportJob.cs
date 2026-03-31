using BlueBerryFinance.API.Infrastructure.Messaging;

namespace BlueBerryFinance.API.Infrastructure.Jobs
{
    /// <summary>
    /// On the 1st of each month publishes a monthly-report generation job for every active user.
    /// </summary>
    public class MonthlyReportJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonthlyReportJob> _logger;

        public MonthlyReportJob(IServiceScopeFactory scopeFactory, ILogger<MonthlyReportJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MonthlyReportJob started");

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = TimeUntilNextRun();
                _logger.LogDebug("MonthlyReportJob sleeping {Hours:F1}h until next run", delay.TotalHours);

                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                await RunAsync(stoppingToken);
            }
        }

        private async Task RunAsync(CancellationToken ct)
        {
            var now   = DateTime.UtcNow;
            var year  = now.Month == 1 ? now.Year - 1 : now.Year;
            var month = now.Month == 1 ? 12 : now.Month - 1;

            _logger.LogInformation("MonthlyReportJob running for {Year}/{Month}", year, month);

            try
            {
                using var scope     = _scopeFactory.CreateScope();
                var publisher       = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
                var db              = scope.ServiceProvider.GetRequiredService<Data.Context.AppDbContext>();

                var userIds = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                    .ToListAsync(
                        db.Users.Select(u => u.Id),
                        ct);

                foreach (var userId in userIds)
                {
                    await publisher.PublishAsync(
                        QueueNames.MonthlyReport,
                        new { UserId = userId, Year = year, Month = month },
                        ct);
                }

                _logger.LogInformation("MonthlyReportJob queued {Count} jobs", userIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MonthlyReportJob failed");
            }
        }

        /// <summary>Returns the delay until 00:05 UTC on the 1st of next month.</summary>
        private static TimeSpan TimeUntilNextRun()
        {
            var now    = DateTime.UtcNow;
            var next   = new DateTime(now.Year, now.Month, 1, 0, 5, 0, DateTimeKind.Utc).AddMonths(1);
            var delay  = next - now;
            return delay > TimeSpan.Zero ? delay : TimeSpan.FromMinutes(1);
        }
    }
}
