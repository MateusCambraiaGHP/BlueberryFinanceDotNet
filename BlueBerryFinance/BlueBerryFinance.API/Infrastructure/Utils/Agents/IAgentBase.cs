namespace BlueBerryFinance.API.Infrastructure.Utils.Agents
{
    public interface IAgentBase<T>
    {
        Task<T?> AskAsync(string prompt);
    }
}
