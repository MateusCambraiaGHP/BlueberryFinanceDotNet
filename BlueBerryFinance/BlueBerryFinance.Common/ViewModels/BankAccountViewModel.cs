namespace BlueBerryFinance.Common.ViewModels
{
    public class BankAccountViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Bank { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public bool Active { get; set; }

        public BankAccountViewModel(
            Guid id,
            Guid userId,
            string name,
            string bank,
            string country,
            string currencyCode,
            string currencySymbol,
            decimal balance,
            bool active)
        {
            Id = id;
            UserId = userId;
            Name = name;
            Bank = bank;
            Country = country;
            CurrencyCode = currencyCode;
            CurrencySymbol = currencySymbol;
            Balance = balance;
            Active = active;
        }
    }
}
