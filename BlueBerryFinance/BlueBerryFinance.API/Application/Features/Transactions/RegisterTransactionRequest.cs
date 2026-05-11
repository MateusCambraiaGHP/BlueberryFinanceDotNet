using BlueBerryFinance.API.Domain.Entities.Enums;

namespace BlueBerryFinance.API.Application.Features.Transactions
{
    public class RegisterTransactionRequest
    {
        public Guid BankAccountId { get; set; }
        public Guid StoreId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid CurrencyId { get; set; }
        public Guid? FixedExpenseId { get; set; }
        public OriginType OriginType { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionSource Source { get; set; } = TransactionSource.Manual;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string? ImageUrl { get; set; }
    }
}
