using BlueBerryFinance.API.Application.Requests;
using BlueBerryFinance.API.Application.Responses;

namespace BlueBerryFinance.API.Application.Handlers.Interfaces
{
    public interface ITransactionsHandler : IRequestHandler<FinancialAnalysisRequest, FinancialAnalysisResponse> { }
}
