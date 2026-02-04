using BlueBerryFinance.API.Application.Responses;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Factories.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Helpers.Interfaces;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.GeneralPurpose
{
    public class FinantialAssistantAgent : AgentBase<FinancialAnalysisResponse>, IFinantialAssistantAgent
    {
        public FinantialAssistantAgent(
            IAIAgentFactory agentFactory, 
            IPromptLoader promptLoader)
            : base(agentFactory, promptLoader.Load("Infrastructure.Utils.Agents.Finance.Prompts.FinantialAssistant.md")) { }
    }
}
