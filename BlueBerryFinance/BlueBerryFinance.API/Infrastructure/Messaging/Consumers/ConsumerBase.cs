using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BlueBerryFinance.API.Infrastructure.Messaging.Consumers
{
    public abstract class ConsumerBase<T> : BackgroundService
    {
        private readonly RabbitMqOptions _opts;
        private readonly ILogger _logger;
        private readonly string _queue;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        protected ConsumerBase(RabbitMqOptions opts, ILogger logger, string queue)
        {
            _opts  = opts;
            _logger = logger;
            _queue  = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Consumer {Queue} starting", _queue);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConsumeAsync(stoppingToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Consumer {Queue} faulted — retrying in 10s", _queue);
                    await Task.Delay(10_000, stoppingToken);
                }
            }
        }

        private async Task ConsumeAsync(CancellationToken ct)
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
                { "x-dead-letter-routing-key", $"{_queue}.dead-letter" }
            };

            await channel.QueueDeclareAsync(
                _queue, durable: true, exclusive: false, autoDelete: false,
                arguments: dlxArgs, cancellationToken: ct);

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: ct);

            var tcs = new TaskCompletionSource();
            ct.Register(() => tcs.TrySetResult());

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var body    = Encoding.UTF8.GetString(ea.Body.Span);
                    var message = JsonSerializer.Deserialize<T>(body, _jsonOpts);

                    if (message is not null)
                        await HandleAsync(message, ct);

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Consumer {Queue} failed to process message — nack", _queue);
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: ct);
                }
            };

            await channel.BasicConsumeAsync(_queue, autoAck: false, consumer: consumer, cancellationToken: ct);

            _logger.LogInformation("Consumer {Queue} listening", _queue);
            await tcs.Task;
        }

        protected abstract Task HandleAsync(T message, CancellationToken ct);
    }
}
