using Microsoft.Agents.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Factories.Interfaces
{
    public interface IAIAgentFactory
    {
        AIAgent Create(string instructions, string? model = null);
    }
}
