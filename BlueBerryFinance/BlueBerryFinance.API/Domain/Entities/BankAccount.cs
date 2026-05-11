using BlueBerryFinance.API.Domain.Entities.Enums;
using BlueBerryFinance.Common.DomainObjects;

namespace BlueBerryFinance.API.Domain.Entities
{
    public class BankAccount : EntityBase
    {
        public Guid UserId { get; set; }
        public Guid CurrencyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Bank Bank { get; set; }
        public Country Country { get; set; }
        public decimal Balance { get; set; }

        public User User { get; set; } = null!;
        public Currency Currency { get; set; } = null!;

        public BankAccount() { }
    }
}
