using BlueBerryFinance.API.Application.Features.Chat;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces
{
    [Obsolete("Use IBlueberryFinanceAgent instead.")]
    public interface IFinantialAssistantAgent : IAgentBase<FinancialAnalysisResponse> { }
}
