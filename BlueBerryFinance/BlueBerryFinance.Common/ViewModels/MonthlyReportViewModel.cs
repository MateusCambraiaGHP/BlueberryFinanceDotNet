namespace BlueBerryFinance.Common.ViewModels
{
    public class MonthlyReportViewModel
    {
        public int Year  { get; set; }
        public int Month { get; set; }

        public IReadOnlyList<CurrencyTotalViewModel> Totals      { get; set; } = [];
        public IReadOnlyList<CategoryBreakdownViewModel> Categories { get; set; } = [];
        public IReadOnlyList<StoreBreakdownViewModel> Stores      { get; set; } = [];
    }

    public class CurrencyTotalViewModel
    {
        public string CurrencyCode   { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public decimal TotalIncome   { get; set; }
        public decimal TotalExpense  { get; set; }
        public decimal Balance       { get; set; }
    }

    public class CategoryBreakdownViewModel
    {
        public string CategoryName  { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public string Type          { get; set; } = string.Empty;
        public string CurrencyCode  { get; set; } = string.Empty;
        public string CurrencySymbol{ get; set; } = string.Empty;
        public decimal Total        { get; set; }
        public int TransactionCount { get; set; }
    }

    public class StoreBreakdownViewModel
    {
        public string StoreName     { get; set; } = string.Empty;
        public string CurrencyCode  { get; set; } = string.Empty;
        public string CurrencySymbol{ get; set; } = string.Empty;
        public decimal Total        { get; set; }
        public int TransactionCount { get; set; }
    }
}
