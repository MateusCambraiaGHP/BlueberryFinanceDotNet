using BlueBerryFinance.API.Data.Entities.enums;
using BlueBerryFinance.Common.DomainObjects;

namespace BlueBerryFinance.API.Data.Entities
{
    public class FixedExpense : EntityBase
    {
        public int CurrencyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int DayOfMonth { get; set; }
        public byte IsRecurring { get; set; }
        public Store Store { get; set; }
        public Currency Currency { get; set; }

        public FixedExpense() { }
    }
}
