using BlueBerryFinance.API.Infrastructure.Utils.Enums;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Factories.Interfaces
{
    public interface IAIAgentFactory
    {
        AIAgent Create(
            AIProvider provider = AIProvider.OpenAI,
            ChatOptions? chatOptions = null);
    }
}
