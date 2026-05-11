namespace BlueBerryFinance.API.Application.Features.Transaction
{
    public class ListTransactionsRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Guid? BankAccountId { get; set; }
        public Guid? CategoryId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Search { get; set; }
    }
}
