using BlueBerryFinance.API.Data.Entities.enums;
using BlueBerryFinance.Commom.Entities;

namespace BlueBerryFinance.API.Data.Entities
{
    public class BankAccount : EntityBase
    {
        public int CurrencyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public Currency Currency { get; set; }

        public BankAccount() { }
    }
}
