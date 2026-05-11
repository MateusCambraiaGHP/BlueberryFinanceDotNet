namespace BlueBerryFinance.Common.ViewModels
{
    public class StoreBreakdownViewModel
    {
        public string StoreName     { get; set; } = string.Empty;
        public string CurrencyCode  { get; set; } = string.Empty;
        public string CurrencySymbol{ get; set; } = string.Empty;
        public decimal Total        { get; set; }
        public int TransactionCount { get; set; }
    }
}
