using BlueBerryFinance.API.Infrastructure.Utils.Agents.Enums;
using Microsoft.Agents.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Factories.Interfaces
{
    public interface IAIAgentFactory
    {
        AIAgent Create(
            string instructions, 
            string model = OpenAIModels.Gpt41, 
            AIProvider provider = AIProvider.OpenAI);
    }
}
