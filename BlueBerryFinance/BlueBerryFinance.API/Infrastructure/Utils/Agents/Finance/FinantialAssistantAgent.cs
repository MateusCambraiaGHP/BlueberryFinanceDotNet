using BlueBerryFinance.API.Application.Features.Chats;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Factories.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Helpers.Interfaces;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance
{
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
