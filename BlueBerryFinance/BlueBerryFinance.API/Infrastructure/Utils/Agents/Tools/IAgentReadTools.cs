using Microsoft.Extensions.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools
{
    public interface IAgentReadTools
    {
        IList<AITool> GetTools();
    }
}
