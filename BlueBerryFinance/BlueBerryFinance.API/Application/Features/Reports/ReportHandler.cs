using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Domain.Entities;
using BlueBerryFinance.API.Domain.Entities.Enums;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Features.Reports
{
    public class ReportHandler : IReportHandler
    {
        private readonly AppDbContext _db;

        public ReportHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<BaseResponse<MonthlyReportViewModel>> GetMonthlyAsync(int year, int month, Guid userId)
        {
            var (from, to) = GetMonthRange(year, month);

            var transactions = await FetchTransactionsAsync(from, to, userId);

            var totals = BuildTotals(transactions);
            var categories = BuildCategories(transactions);
            var stores = BuildStores(transactions);

            var viewModel = new MonthlyReportViewModel(year, month, totals, categories, stores);

            return BaseResponse<MonthlyReportViewModel>.Ok(viewModel);
        }

        private static (DateTime From, DateTime To) GetMonthRange(int year, int month)
        {
            var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            return (from, from.AddMonths(1));
        }

        private async Task<List<TransactionSummaryViewModel>> FetchTransactionsAsync(
    DateTime from, DateTime to, Guid userId)
        {
            return await _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId
                         && t.TransactionDate >= from
                         && t.TransactionDate < to)
                .Select(t => new TransactionSummaryViewModel(
                    (int)t.TransactionType,
                    t.Amount,
                    t.Category.Name,
                    t.Category.Color ?? "#888888",
                    t.Category.Type,
                    t.Store.Name,
                    t.Currency.Code.ToString(),
                    t.Currency.Symbol))
                .ToListAsync();
        }

        private static List<CurrencyTotalViewModel> BuildTotals(
            List<TransactionSummaryViewModel> transactions)
        {
            return transactions
                .GroupBy(t => t.CurrencyCode)
                .Select(g =>
                {
                    var income = g.Where(t => t.TransactionType == (int)TransactionType.Income)
                        .Sum(t => t.Amount);
                    var expense = g.Where(t => t.TransactionType == (int)TransactionType.Expense)
                        .Sum(t => t.Amount);

                    return new CurrencyTotalViewModel
                    {
                        CurrencyCode = g.Key,
                        CurrencySymbol = g.First().CurrencySymbol,
                        TotalIncome = income,
                        TotalExpense = expense,
                        Balance = income - expense
                    };
                })
                .OrderBy(x => x.CurrencyCode)
                .ToList();
        }

        private static List<CategoryBreakdownViewModel> BuildCategories(List<TransactionSummaryViewModel> transactions)
        {
            return transactions
                .GroupBy(t => (t.CategoryName, t.CategoryColor, t.CategoryType, t.CurrencyCode, t.CurrencySymbol))
                .Select(g => new CategoryBreakdownViewModel
                {
                    CategoryName = g.Key.CategoryName,
                    CategoryColor = g.Key.CategoryColor,
                    Type = g.Key.CategoryType,
                    CurrencyCode = g.Key.CurrencyCode,
                    CurrencySymbol = g.Key.CurrencySymbol,
                    Total = g.Sum(t => t.Amount),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();
        }

        private static List<StoreBreakdownViewModel> BuildStores(
            List<TransactionSummaryViewModel> transactions)
        {
            return transactions
                .GroupBy(t => (t.StoreName, t.CurrencyCode, t.CurrencySymbol))
                .Select(g => new StoreBreakdownViewModel
                {
                    StoreName = g.Key.StoreName,
                    CurrencyCode = g.Key.CurrencyCode,
                    CurrencySymbol = g.Key.CurrencySymbol,
                    Total = g.Sum(t => t.Amount),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(10)
                .ToList();
        }
    }
}
