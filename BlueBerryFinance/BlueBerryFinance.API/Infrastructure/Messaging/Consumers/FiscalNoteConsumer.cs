using BlueBerryFinance.API.Infrastructure.Messaging;
using BlueBerryFinance.API.Infrastructure.Messaging.Consumers;
using Microsoft.Extensions.Options;

namespace BlueBerryFinance.API.Infrastructure.Messaging.Consumers
{
    /// <summary>
    /// Consumes fiscal-note processing jobs from the queue.
    /// Phase 4 will inject IFiscalNoteAgent here to perform OCR + transaction creation.
    /// </summary>
    public class FiscalNoteConsumer : ConsumerBase<FiscalNoteMessage>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public FiscalNoteConsumer(
            IOptions<RabbitMqOptions> opts,
            ILogger<FiscalNoteConsumer> logger,
            IServiceScopeFactory scopeFactory)
            : base(opts.Value, logger, QueueNames.FiscalNote)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task HandleAsync(FiscalNoteMessage message, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<FiscalNoteConsumer>>();

            logger.LogInformation(
                "FiscalNote job received: userId={UserId} imageUrl={ImageUrl}",
                message.UserId, message.ImageUrl);

            // TODO Phase 4: resolve IFiscalNoteAgent, call ProcessAsync(message)
            await Task.CompletedTask;
        }
    }

    public record FiscalNoteMessage(Guid UserId, string ImageUrl, string CorrelationId);
}
