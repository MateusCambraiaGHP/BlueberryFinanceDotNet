using BlueBerryFinance.API.Application.Features.Chat;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Factories.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Helpers.Interfaces;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance
{
    /// <summary>
    /// Blueberry Finance orchestrator agent.
    /// Streaming chat orchestration is handled by ChatHandler (using IOrchestratorTools).
    /// Structured financial analysis is handled by FinancialAnalysisTool.
    /// This class fulfils the IBlueberryFinanceAgent contract for direct structured calls.
    /// </summary>
    public class BlueberryFinanceAgent : AgentBase<FinancialAnalysisResponse>, IBlueberryFinanceAgent
    {
        public BlueberryFinanceAgent(
            IAIAgentFactory agentFactory,
            IPromptLoader promptLoader)
            : base(
                agentFactory,
                promptLoader.Load("Infrastructure.Utils.Agents.Finance.Prompts.FinantialAssistant.md")) { }
    }
}
