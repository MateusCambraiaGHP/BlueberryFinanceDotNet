using BlueBerryFinance.API.Application.Features.Report;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Data.Entities.Enums;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Features.Report
{
    public class ReportHandler : IReportHandler
    {
        private readonly AppDbContext _db;

        public ReportHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<MonthlyReportViewModel> GetMonthlyAsync(
            int year, int month, Guid userId, CancellationToken ct = default)
        {
            var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var to   = from.AddMonths(1);

            var transactions = await _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId
                         && t.TransactionDate >= from
                         && t.TransactionDate <  to)
                .Select(t => new
                {
                    t.TransactionType,
                    t.Amount,
                    CategoryName  = t.Category.Name,
                    CategoryColor = t.Category.Color ?? "#888888",
                    CategoryType  = t.Category.Type,
                    StoreName     = t.Store.Name,
                    CurrencyCode  = t.Currency.Code.ToString(),
                    CurrencySymbol= t.Currency.Symbol
                })
                .ToListAsync(ct);

            var totals = transactions
                .GroupBy(t => t.CurrencyCode)
                .Select(g =>
                {
                    var income  = g.Where(t => t.TransactionType == TransactionType.Income) .Sum(t => t.Amount);
                    var expense = g.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount);
                    return new CurrencyTotalViewModel
                    {
                        CurrencyCode   = g.Key,
                        CurrencySymbol = g.First().CurrencySymbol,
                        TotalIncome    = income,
                        TotalExpense   = expense,
                        Balance        = income - expense
                    };
                })
                .OrderBy(x => x.CurrencyCode)
                .ToList();

            var categories = transactions
                .GroupBy(t => (t.CategoryName, t.CategoryColor, t.CategoryType, t.CurrencyCode, t.CurrencySymbol))
                .Select(g => new CategoryBreakdownViewModel
                {
                    CategoryName   = g.Key.CategoryName,
                    CategoryColor  = g.Key.CategoryColor,
                    Type           = g.Key.CategoryType,
                    CurrencyCode   = g.Key.CurrencyCode,
                    CurrencySymbol = g.Key.CurrencySymbol,
                    Total          = g.Sum(t => t.Amount),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            var stores = transactions
                .GroupBy(t => (t.StoreName, t.CurrencyCode, t.CurrencySymbol))
                .Select(g => new StoreBreakdownViewModel
                {
                    StoreName      = g.Key.StoreName,
                    CurrencyCode   = g.Key.CurrencyCode,
                    CurrencySymbol = g.Key.CurrencySymbol,
                    Total          = g.Sum(t => t.Amount),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(10)
                .ToList();

            return new MonthlyReportViewModel
            {
                Year       = year,
                Month      = month,
                Totals     = totals,
                Categories = categories,
                Stores     = stores
            };
        }
    }
}
