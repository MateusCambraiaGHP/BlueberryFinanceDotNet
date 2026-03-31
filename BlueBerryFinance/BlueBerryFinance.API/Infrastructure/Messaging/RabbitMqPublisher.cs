using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text.Json;

namespace BlueBerryFinance.API.Infrastructure.Messaging
{
    public class RabbitMqPublisher : IMessagePublisher
    {
        private readonly RabbitMqOptions _opts;
        private readonly ILogger<RabbitMqPublisher> _logger;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public RabbitMqPublisher(IOptions<RabbitMqOptions> opts, ILogger<RabbitMqPublisher> logger)
        {
            _opts = opts.Value;
            _logger = logger;
        }

        public async Task PublishAsync<T>(string queue, T message, CancellationToken ct = default)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName    = _opts.Host,
                    UserName    = _opts.User,
                    Password    = _opts.Password,
                    VirtualHost = _opts.VirtualHost,
                    AutomaticRecoveryEnabled = true
                };

                await using var connection = await factory.CreateConnectionAsync(ct);
                await using var channel    = await connection.CreateChannelAsync(cancellationToken: ct);

                var dlxArgs = new Dictionary<string, object?>
                {
                    { "x-dead-letter-exchange",    "" },
                    { "x-dead-letter-routing-key", $"{queue}.dead-letter" }
                };

                await channel.QueueDeclareAsync(
                    queue, durable: true, exclusive: false, autoDelete: false,
                    arguments: dlxArgs, cancellationToken: ct);

                var body  = JsonSerializer.SerializeToUtf8Bytes(message, _jsonOpts);
                var props = new BasicProperties { DeliveryMode = DeliveryModes.Persistent };

                await channel.BasicPublishAsync(
                    exchange: "", routingKey: queue,
                    mandatory: false, basicProperties: props,
                    body: body, cancellationToken: ct);

                _logger.LogDebug("Published message to queue {Queue}", queue);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish to queue {Queue} — RabbitMQ may be unavailable", queue);
            }
        }
    }
}
