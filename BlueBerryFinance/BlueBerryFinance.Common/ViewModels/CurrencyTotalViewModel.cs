namespace BlueBerryFinance.Common.ViewModels
{
    public class CurrencyTotalViewModel
    {
        public string CurrencyCode   { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public decimal TotalIncome   { get; set; }
        public decimal TotalExpense  { get; set; }
        public decimal Balance       { get; set; }
    }
}
