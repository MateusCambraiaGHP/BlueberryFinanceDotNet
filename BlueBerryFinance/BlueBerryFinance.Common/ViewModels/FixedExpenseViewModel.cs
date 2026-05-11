namespace BlueBerryFinance.Common.ViewModels
{
    public class FixedExpenseViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public int DayOfMonth { get; set; }
        public bool IsRecurring { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public bool Active { get; set; }

        public FixedExpenseViewModel(
            Guid id,
            Guid userId,
            string name,
            string description,
            decimal amount,
            string currencyCode,
            string currencySymbol,
            int dayOfMonth,
            bool isRecurring,
            string storeName,
            bool active)
        {
            Id = id;
            UserId = userId;
            Name = name;
            Description = description;
            Amount = amount;
            CurrencyCode = currencyCode;
            CurrencySymbol = currencySymbol;
            DayOfMonth = dayOfMonth;
            IsRecurring = isRecurring;
            StoreName = storeName;
            Active = active;
        }
    }
}
