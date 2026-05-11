namespace BlueBerryFinance.API.Application.Features.Chat
{
    public interface IChatHandler
    {
        IAsyncEnumerable<string> StreamAsync(string prompt, bool includeTools = true);
    }
}
