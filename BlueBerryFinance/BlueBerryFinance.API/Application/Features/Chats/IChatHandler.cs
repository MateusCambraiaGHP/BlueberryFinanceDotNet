namespace BlueBerryFinance.API.Application.Features.Chats
{
    public interface IChatHandler
    {
        IAsyncEnumerable<string> StreamAsync(string prompt, bool includeTools = true);
    }
}
