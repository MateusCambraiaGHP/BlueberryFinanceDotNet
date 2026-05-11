using BlueBerryFinance.Common.DomainObjects;

namespace BlueBerryFinance.API.Domain.Entities
{
    public class TransactionItem : EntityBase
    {
        public Guid TransactionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public Transaction Transaction { get; set; } = null!;

        public TransactionItem() { }
    }
}
