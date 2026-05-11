using BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces;
using Microsoft.Extensions.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools
{
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
