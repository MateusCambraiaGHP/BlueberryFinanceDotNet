namespace BlueBerryFinance.Common.ViewModels
{
    public class TransactionViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BankAccountId { get; set; }
        public string BankAccountName { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryColor { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public string TransactionType { get; set; }
        public string Source { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? ImageUrl { get; set; }
        public string CorrelationId { get; set; }
        public DateTime InsertionDate { get; set; }
        public IList<TransactionItemViewModel> Items { get; set; }

        public TransactionViewModel(
            Guid id,
            Guid userId,
            Guid bankAccountId,
            string bankAccountName,
            Guid storeId,
            string storeName,
            Guid categoryId,
            string categoryName,
            string categoryColor,
            string currencyCode,
            string currencySymbol,
            string transactionType,
            string source,
            decimal amount,
            string description,
            DateTime transactionDate,
            string? imageUrl,
            string correlationId,
            DateTime insertionDate)
        {
            Id = id;
            UserId = userId;
            BankAccountId = bankAccountId;
            BankAccountName = bankAccountName;
            StoreId = storeId;
            StoreName = storeName;
            CategoryId = categoryId;
            CategoryName = categoryName;
            CategoryColor = categoryColor;
            CurrencyCode = currencyCode;
            CurrencySymbol = currencySymbol;
            TransactionType = transactionType;
            Source = source;
            Amount = amount;
            Description = description;
            TransactionDate = transactionDate;
            ImageUrl = imageUrl;
            CorrelationId = correlationId;
            InsertionDate = insertionDate;
            Items = [];
        }
    }
}
