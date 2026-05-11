using BlueBerryFinance.API.Application.Features.Chats;
using Microsoft.Extensions.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces
{
    public interface IFinancialAnalysisTool
    {
        AITool GetTool();
        Task<FinancialAnalysisResponse> AnalyzeAsync(string prompt, CancellationToken ct = default);
    }
}
