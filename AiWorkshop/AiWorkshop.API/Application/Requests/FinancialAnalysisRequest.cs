using BlueBerryFinance.API.Infrastructure.Utils.Enums;
using System.ComponentModel.DataAnnotations;

namespace BlueBerryFinance.API.Application.Requests
{
    public class FinancialAnalysisRequest
    {
        public string Prompt { get; init; } = string.Empty;
        public AIProvider Provider { get; init; } = AIProvider.OpenAI;
    }
}
