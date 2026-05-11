using BlueBerryFinance.API.Application.Features.Common;

namespace BlueBerryFinance.API.Application.Features.Transactions
{
    public class GetTransactionsFilterRequest : BaseRequest
    {
        public Guid? Id { get; set; }
        public Guid? BankAccountId { get; set; }
        public Guid? CategoryId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Search { get; set; }
    }
}
