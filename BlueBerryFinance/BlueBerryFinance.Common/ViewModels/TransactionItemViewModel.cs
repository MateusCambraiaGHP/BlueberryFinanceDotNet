namespace BlueBerryFinance.Common.ViewModels
{
    public class TransactionItemViewModel
    {
        public Guid Id { get; set; }
        public Guid TransactionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
