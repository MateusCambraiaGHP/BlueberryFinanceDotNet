using System.ComponentModel;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Finance.Responses
{
    public class FinancialAnalysisResponse
    {
        [Description("The main answer to the user's question")]
        public string Answer { get; set; } = string.Empty;
        [Description("Insights derived from the financial data analysis")]
        public string Insights { get; set; } = string.Empty;
        [Description("Recommendations based on the financial analysis")]
        public string Recommendations { get; set; } = string.Empty;
        [Description("A summary of the financial data analyzed")]
        public string DataSummary { get; set; } = string.Empty;
    }
}
