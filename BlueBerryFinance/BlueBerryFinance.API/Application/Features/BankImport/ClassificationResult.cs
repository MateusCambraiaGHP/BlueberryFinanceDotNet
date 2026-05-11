using System.ComponentModel;

namespace BlueBerryFinance.API.Application.Features.BankImport
{
    public class ClassificationResult
    {
        [Description("Transaction type: Income or Expense")]
        public string Type { get; set; } = string.Empty;

        [Description("Origin type: Person, Company, or Store")]
        public string OriginType { get; set; } = string.Empty;

        [Description("Cleaned transaction description")]
        public string Description { get; set; } = string.Empty;

        [Description("Suggested category name (e.g. Food & Dining, Transport, Salary)")]
        public string CategorySuggestion { get; set; } = string.Empty;
    }
}
