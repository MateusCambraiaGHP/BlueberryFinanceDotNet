using BlueBerryFinance.API.Domain.Entities.Enums;
using BlueBerryFinance.Common.DomainObjects;

namespace BlueBerryFinance.API.Domain.Entities
{
    public class Transaction : EntityBase
    {
        public Guid UserId { get; set; }
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
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

        public User User { get; set; } = null!;
        public BankAccount BankAccount { get; set; } = null!;
        public Store Store { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public Currency Currency { get; set; } = null!;
        public FixedExpense? FixedExpense { get; set; }
        public ICollection<TransactionItem> Items { get; set; } = [];

        public Transaction() { }
    }
}
