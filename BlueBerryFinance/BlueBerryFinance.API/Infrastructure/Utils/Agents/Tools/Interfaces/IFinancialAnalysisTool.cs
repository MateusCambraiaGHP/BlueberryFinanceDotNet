using BlueBerryFinance.API.Application.Features.Chat;
using Microsoft.Extensions.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces
{
    /// <summary>
    /// Tool that queries financial data and returns structured analysis, insights, and recommendations.
    /// Exposes: financial_analysis (AITool for orchestrator), AnalyzeAsync (for direct handler use).
    /// </summary>
    public interface IFinancialAnalysisTool
    {
        AITool GetTool();
        Task<FinancialAnalysisResponse> AnalyzeAsync(string prompt, CancellationToken ct = default);
    }
}
