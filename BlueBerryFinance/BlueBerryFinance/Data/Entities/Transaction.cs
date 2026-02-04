using BlueBerryFinance.API.Data.Entities.enums;
using BlueBerryFinance.Commom.Entities;

namespace BlueBerryFinance.API.Data.Entities
{
    public class Transaction : EntityBase
    {
        public int CurrencyId { get; set; }
        public int StoreId { get; set; }
        public int CategoryId { get; set; }
        public int BankAccountId { get; set; }
        public int FixedExpenseId { get; set; }
        public int OriginTypeId { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public Store Store { get; set; }
        public Category Category { get; set; }
        public Currency Currency { get; set; }
        public BankAccount BankAccount { get; set; }
        public FixedExpense FixedExpense { get; set; }
        public OriginType OriginType { get; set; }

        public Transaction() { }
    }
}
