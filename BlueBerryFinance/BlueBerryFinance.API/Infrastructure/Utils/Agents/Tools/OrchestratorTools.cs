using BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces;
using Microsoft.Extensions.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools
{
    /// <summary>
    /// Assembles the four tools exposed to the Blueberry Finance orchestrator agent.
    /// DB Save Tool and DB Query Tool are NOT included here — they are internal to each tool.
    /// </summary>
    public class OrchestratorTools : IOrchestratorTools
    {
        private readonly IIncomeExpenseSaverTool _saver;
        private readonly IExtractProcessorTool _extractor;
        private readonly IImageAnalyzerTool _imageAnalyzer;
        private readonly IFinancialAnalysisTool _financialAnalysis;

        public OrchestratorTools(
            IIncomeExpenseSaverTool saver,
            IExtractProcessorTool extractor,
            IImageAnalyzerTool imageAnalyzer,
            IFinancialAnalysisTool financialAnalysis)
        {
            _saver = saver;
            _extractor = extractor;
            _imageAnalyzer = imageAnalyzer;
            _financialAnalysis = financialAnalysis;
        }

        public IList<AITool> GetTools() =>
        [
            .. _saver.GetTools(),
            _extractor.GetTool(),
            _imageAnalyzer.GetTool(),
            _financialAnalysis.GetTool()
        ];
    }
}
