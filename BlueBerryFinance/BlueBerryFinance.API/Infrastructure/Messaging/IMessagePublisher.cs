namespace BlueBerryFinance.API.Infrastructure.Messaging
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(string queue, T message, CancellationToken ct = default);
    }
}
