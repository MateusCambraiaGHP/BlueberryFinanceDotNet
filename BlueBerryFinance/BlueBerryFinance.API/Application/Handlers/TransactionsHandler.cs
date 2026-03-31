using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Application.Requests;
using BlueBerryFinance.API.Application.Responses;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces;

namespace BlueBerryFinance.API.Application.Handlers
{
    public class TransactionsHandler : ITransactionsHandler
    {
        private readonly IFinancialAnalysisTool _analysisTool;

        public TransactionsHandler(IFinancialAnalysisTool analysisTool)
        {
            _analysisTool = analysisTool;
        }

        public async Task<FinancialAnalysisResponse> HandleAsync(
            FinancialAnalysisRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return new FinancialAnalysisResponse();

            return await _analysisTool.AnalyzeAsync(request.Prompt, cancellationToken);
        }
    }
}
