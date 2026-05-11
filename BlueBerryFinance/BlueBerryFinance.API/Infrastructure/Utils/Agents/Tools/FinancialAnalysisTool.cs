using BlueBerryFinance.API.Application.Features.Chats;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Factories.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Helpers.Interfaces;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools
{
    public class FinancialAnalysisTool : AgentBase<FinancialAnalysisResponse>, IFinancialAnalysisTool
    {
        public FinancialAnalysisTool(
            IAIAgentFactory agentFactory,
            IPromptLoader promptLoader,
            IAgentReadTools readTools)
            : base(
                agentFactory,
                promptLoader.Load("Infrastructure.Utils.Agents.Finance.Prompts.FinantialAssistant.md"),
                readTools.GetTools()) { }

        public AITool GetTool() => AIFunctionFactory.Create(
            RunAnalysisAsync,
            "financial_analysis",
            "Analyzes the user's financial data and returns insights, spending patterns, savings opportunities, and recommendations. Always call this for financial questions.");

        public async Task<FinancialAnalysisResponse> AnalyzeAsync(
            string prompt, CancellationToken ct = default)
            => await AskAsync(prompt) ?? new FinancialAnalysisResponse();

        private async Task<string> RunAnalysisAsync(
            [Description("The user's full financial question or analysis request")] string prompt)
        {
            var result = await AskAsync(prompt);
            if (result is null)
                return "Financial analysis could not be completed. Please try again.";

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }
}
