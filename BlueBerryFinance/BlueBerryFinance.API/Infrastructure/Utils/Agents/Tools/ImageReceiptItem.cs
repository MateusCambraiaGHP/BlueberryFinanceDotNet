namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools
{
    public partial class ImageAnalyzerTool
    {
        private sealed class ImageReceiptItem
        {
            public string Name { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
        }
    }
}
