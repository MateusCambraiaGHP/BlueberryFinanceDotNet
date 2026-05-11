namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools
{
    public partial class ImageAnalyzerTool
    {
        private sealed class ImageReceiptData
        {
            public string? ShopName { get; set; }
            public decimal? TotalAmount { get; set; }
            public string? Currency { get; set; }
            public string? Country { get; set; }
            public string? Description { get; set; }
            public string? TransactionDate { get; set; }
            public IList<ImageReceiptItem>? Items { get; set; }
        }
    }
}
