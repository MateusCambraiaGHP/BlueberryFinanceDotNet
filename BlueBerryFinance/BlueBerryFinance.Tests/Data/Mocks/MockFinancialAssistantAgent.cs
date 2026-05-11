using BlueBerryFinance.API.Application.Features.Chat;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces;

namespace BlueBerryFinance.Tests.Data.Mocks
{
    public class MockFinancialAssistantAgent : IFinantialAssistantAgent
    {
        public Task<FinancialAnalysisResponse?> AskAsync(string prompt)
            => Task.FromResult<FinancialAnalysisResponse?>(new FinancialAnalysisResponse { Answer = "Mock answer" });
    }
}
