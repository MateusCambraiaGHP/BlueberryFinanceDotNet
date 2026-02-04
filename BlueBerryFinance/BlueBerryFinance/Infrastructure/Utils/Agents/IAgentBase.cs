using BlueBerryFinance.API.Infrastructure.Utils.Enums;
using Microsoft.Agents.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents
{
    public interface IAgentBase<T>
    {
        Task<T?> AskAsync(string prompt, Func<AgentResponse, T> parser, AIProvider provider = AIProvider.OpenAI);
    }
}
