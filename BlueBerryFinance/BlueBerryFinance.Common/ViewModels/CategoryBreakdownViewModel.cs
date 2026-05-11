namespace BlueBerryFinance.Common.ViewModels
{
    public class CategoryBreakdownViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int TransactionCount { get; set; }
    }
}
