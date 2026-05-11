using BlueBerryFinance.API.Domain.Entities.Enums;

namespace BlueBerryFinance.API.Application.Features.BankAccounts
{
    public class RegisterBankAccountRequest
    {
        public Guid CurrencyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Bank Bank { get; set; }
        public Country Country { get; set; }
        public decimal InitialBalance { get; set; }
    }
}
