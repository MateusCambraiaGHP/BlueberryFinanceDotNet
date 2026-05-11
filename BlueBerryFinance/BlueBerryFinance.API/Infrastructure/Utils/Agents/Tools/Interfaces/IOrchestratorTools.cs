using Microsoft.Extensions.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces
{
    /// <summary>
    /// Provides the four tools exposed to the Blueberry Finance orchestrator agent.
    /// The orchestrator cannot access DB read/write tools directly — all DB access
    /// is handled internally by each tool.
    /// </summary>
    public interface IOrchestratorTools
    {
        IList<AITool> GetTools();
    }
}
