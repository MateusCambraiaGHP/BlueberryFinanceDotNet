using Microsoft.Extensions.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces
{
    public interface IOrchestratorTools
    {
        IList<AITool> GetTools();
    }
}
