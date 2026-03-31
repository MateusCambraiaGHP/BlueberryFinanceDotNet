using BlueBerryFinance.API.Application.Responses;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces
{
    /// <summary>
    /// Marker interface for the Blueberry Finance orchestrator agent.
    /// Orchestration logic lives in ChatHandler (streaming) and FinancialAnalysisTool (structured).
    /// </summary>
    public interface IBlueberryFinanceAgent : IAgentBase<FinancialAnalysisResponse> { }
}
