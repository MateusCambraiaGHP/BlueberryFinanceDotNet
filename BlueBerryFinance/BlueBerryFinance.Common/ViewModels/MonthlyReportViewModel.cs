namespace BlueBerryFinance.Common.ViewModels
{
    public class MonthlyReportViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public IReadOnlyList<CurrencyTotalViewModel> Totals { get; set; } = [];
        public IReadOnlyList<CategoryBreakdownViewModel> Categories { get; set; } = [];
        public IReadOnlyList<StoreBreakdownViewModel> Stores { get; set; } = [];

        public MonthlyReportViewModel() { }

        public MonthlyReportViewModel(
            int year,
            int month,
            List<CurrencyTotalViewModel> totals,
            List<CategoryBreakdownViewModel> categories,
            List<StoreBreakdownViewModel> stores)
        {
            Year = year;
            Month = month;
            Totals = totals;
            Categories = categories;
            Stores = stores;
        }
    }
}
