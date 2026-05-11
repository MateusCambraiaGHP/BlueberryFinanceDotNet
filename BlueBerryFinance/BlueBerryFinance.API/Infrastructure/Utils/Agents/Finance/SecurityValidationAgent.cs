using BlueBerryFinance.API.Application.Features.AgentApprovals;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools;
using BlueBerryFinance.API.Infrastructure.Utils.Factories.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Helpers.Interfaces;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance
{
    /// <summary>
    /// Validates the security of proposed write operations.
    /// Uses DB Query Tool (read tools) to verify ownership and detect anomalies.
    /// </summary>
    public class SecurityValidationAgent : AgentBase<SecurityValidationResult>, ISecurityValidationAgent
    {
        public SecurityValidationAgent(
            IAIAgentFactory agentFactory,
            IPromptLoader promptLoader,
            IAgentReadTools readTools)
            : base(
                agentFactory,
                promptLoader.Load("Infrastructure.Utils.Agents.Finance.Prompts.SecurityValidation.md"),
                readTools.GetTools()) { }
    }
}
