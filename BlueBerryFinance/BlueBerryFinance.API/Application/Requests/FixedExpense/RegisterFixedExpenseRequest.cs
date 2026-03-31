namespace BlueBerryFinance.API.Application.Requests.FixedExpense
{
    public class RegisterFixedExpenseRequest
    {
        public Guid CurrencyId { get; set; }
        public Guid StoreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int DayOfMonth { get; set; }
        public bool IsRecurring { get; set; }
    }
}
