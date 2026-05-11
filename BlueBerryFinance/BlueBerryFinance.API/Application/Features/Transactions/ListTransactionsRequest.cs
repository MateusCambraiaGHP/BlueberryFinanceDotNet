namespace BlueBerryFinance.API.Application.Features.Transactions
{
    public class ListTransactionsRequest
    {
        public Guid? Id { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Guid? BankAccountId { get; set; }
        public Guid? CategoryId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Search { get; set; }
    }
}
