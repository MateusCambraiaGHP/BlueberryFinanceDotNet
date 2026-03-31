namespace BlueBerryFinance.API.Application.Handlers.Interfaces
{
    public interface IChatHandler
    {
        IAsyncEnumerable<string> StreamAsync(string prompt, bool includeTools = true, CancellationToken ct = default);
    }
}
