using BlueBerryFinance.API.Data.Entities.Enums;

namespace BlueBerryFinance.API.Application.Requests.Transaction
{
    public class UpdateTransactionRequest
    {
        public Guid Id { get; set; }
        public Guid StoreId { get; set; }
        public Guid CategoryId { get; set; }
        public OriginType OriginType { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
    }
}
