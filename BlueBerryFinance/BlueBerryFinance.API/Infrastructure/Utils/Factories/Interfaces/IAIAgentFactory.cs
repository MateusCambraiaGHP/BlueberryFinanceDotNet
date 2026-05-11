using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Factories.Interfaces
{
    public interface IAIAgentFactory
    {
        AIAgent Create(ChatOptions? chatOptions = null);
        IChatClient CreateChatClient();
    }
}
