using Microsoft.Extensions.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools
{
    public interface IAgentWriteTools
    {
        IList<AITool> GetTools();
    }
}
