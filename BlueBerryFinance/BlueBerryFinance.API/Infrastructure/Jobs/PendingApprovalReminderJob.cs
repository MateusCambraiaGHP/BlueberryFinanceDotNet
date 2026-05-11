using BlueBerryFinance.API.Domain.Entities.Enums;
using BlueBerryFinance.API.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Infrastructure.Jobs
{
    /// <summary>
    /// Every 4 hours: publishes a notification for each user that has pending agent approvals.
    /// </summary>
    public class PendingApprovalReminderJob : BackgroundService
    {
        private static readonly TimeSpan _interval = TimeSpan.FromHours(4);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PendingApprovalReminderJob> _logger;

        public PendingApprovalReminderJob(
            IServiceScopeFactory scopeFactory,
            ILogger<PendingApprovalReminderJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PendingApprovalReminderJob started");

            // Initial delay — let everything warm up first
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await RunAsync(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task RunAsync(CancellationToken ct)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db          = scope.ServiceProvider.GetRequiredService<Data.Context.AppDbContext>();
                var publisher   = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

                var pending = await db.AgentApprovals
                    .AsNoTracking()
                    .Where(a => a.Status == ApprovalStatus.Pending)
                    .GroupBy(a => a.UserId)
                    .Select(g => new { UserId = g.Key, Count = g.Count() })
                    .ToListAsync(ct);

                foreach (var item in pending)
                {
                    await publisher.PublishAsync(
                        QueueNames.Notification,
                        new
                        {
                            item.UserId,
                            Message = $"You have {item.Count} pending agent approval(s) waiting for your review."
                        },
                        ct);
                }

                if (pending.Count > 0)
                    _logger.LogInformation(
                        "PendingApprovalReminderJob sent reminders for {Count} user(s)", pending.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PendingApprovalReminderJob failed");
            }
        }
    }
}
