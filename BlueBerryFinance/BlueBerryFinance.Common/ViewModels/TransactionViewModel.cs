namespace BlueBerryFinance.Common.ViewModels
{
    public class TransactionViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BankAccountId { get; set; }
        public string BankAccountName { get; set; } = string.Empty;
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string? ImageUrl { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
        public DateTime InsertionDate { get; set; }
    }
}
