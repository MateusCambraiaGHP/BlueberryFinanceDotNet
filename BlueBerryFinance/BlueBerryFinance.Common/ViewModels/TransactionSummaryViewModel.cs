namespace BlueBerryFinance.Common.ViewModels
{
    public class TransactionSummaryViewModel
    {
        public int TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string CategoryName { get; set; }
        public string CategoryColor { get; set; }
        public string CategoryType { get; set; }
        public string StoreName { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }

        public TransactionSummaryViewModel(
            int transactionType,
            decimal amount,
            string categoryName,
            string categoryColor,
            string categoryType,
            string storeName,
            string currencyCode,
            string currencySymbol)
        {
            TransactionType = transactionType;
            Amount = amount;
            CategoryName = categoryName;
            CategoryColor = categoryColor;
            CategoryType = categoryType;
            StoreName = storeName;
            CurrencyCode = currencyCode;
            CurrencySymbol = currencySymbol;
        }
    }
}
