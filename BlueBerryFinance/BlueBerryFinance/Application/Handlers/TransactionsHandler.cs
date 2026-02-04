using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Application.Requests;
using BlueBerryFinance.API.Application.Responses;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Interfaces;
using System.Text.Json;

namespace BlueBerryFinance.API.Application.Handlers
{
    public class TransactionsHandler : ITransactionsHandler
    {
        private readonly IFinantialAssistantAgent _agent;

        public TransactionsHandler(IFinantialAssistantAgent agent)
        {
            _agent = agent;
        }

        public async Task<FinancialAnalysisResponse> HandleAsync(
            FinancialAnalysisRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return new FinancialAnalysisResponse();

            var response = await _agent.AskAsync(request.Prompt, response =>
            {
                var content = response.ToString() ?? string.Empty;

                try
                {
                    return JsonSerializer.Deserialize<FinancialAnalysisResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new FinancialAnalysisResponse { Answer = content };
                }
                catch
                {
                    return new FinancialAnalysisResponse { Answer = content };
                }
            });

            return response;
        }
    }
}
