using BlueBerryFinance.API.Application.Features.Chats;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces
{
    [Obsolete("Use IBlueberryFinanceAgent instead.")]
    public interface IFinantialAssistantAgent : IAgentBase<FinancialAnalysisResponse> { }
}
