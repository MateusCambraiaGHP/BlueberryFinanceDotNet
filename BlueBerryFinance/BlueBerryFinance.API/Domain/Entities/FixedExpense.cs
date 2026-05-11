using BlueBerryFinance.Common.DomainObjects;

namespace BlueBerryFinance.API.Domain.Entities
{
    public class FixedExpense : EntityBase
    {
        public Guid UserId { get; set; }
        public Guid CurrencyId { get; set; }
        public Guid StoreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int DayOfMonth { get; set; }
        public bool IsRecurring { get; set; }

        public User User { get; set; } = null!;
        public Currency Currency { get; set; } = null!;
        public Store Store { get; set; } = null!;

        public FixedExpense() { }
    }
}
