using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools
{
    public class AgentReadTools : IAgentReadTools
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AgentReadTools(AppDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public IList<AITool> GetTools() =>
        [
            AIFunctionFactory.Create(GetTransactionsAsync, "get_transactions",
                "Retrieves the user's financial transactions. Use this to answer any question about spending, income, history, or transaction data."),

            AIFunctionFactory.Create(GetBankAccountsAsync, "get_bank_accounts",
                "Retrieves the user's bank accounts with current balances and currencies.")
        ];

        private async Task<string> GetTransactionsAsync(
            [Description("Filter by transaction type. Allowed values: 'Income', 'Expense'. Leave empty for all.")] string? type = null,
            [Description("Start date filter in ISO 8601 format, e.g. '2025-01-01'. Optional.")] DateTime? dateFrom = null,
            [Description("End date filter in ISO 8601 format, e.g. '2025-12-31'. Optional.")] DateTime? dateTo = null,
            [Description("Maximum number of transactions to return. Default 30, max 100.")] int limit = 30)
        {
            var userId = GetCurrentUserId();
            limit = Math.Clamp(limit, 1, 100);

            var query = _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId);

            if (!string.IsNullOrWhiteSpace(type) &&
                Enum.TryParse<TransactionType>(type, ignoreCase: true, out var tt))
            {
                query = query.Where(t => t.TransactionType == tt);
            }

            if (dateFrom.HasValue)
                query = query.Where(t => t.TransactionDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(t => t.TransactionDate <= dateTo.Value);

            var results = await query
                .OrderByDescending(t => t.TransactionDate)
                .Take(limit)
                .Select(t => new
                {
                    id = t.Id,
                    description = t.Description,
                    amount = t.Amount,
                    transactionType = t.TransactionType.ToString(),
                    date = t.TransactionDate.ToString("yyyy-MM-dd"),
                    category = t.Category.Name,
                    store = t.Store.Name,
                    account = t.BankAccount.Name,
                    currency = t.Currency.Symbol,
                    currencyCode = t.Currency.Code.ToString()
                })
                .ToListAsync();

            return JsonSerializer.Serialize(results);
        }

        private async Task<string> GetBankAccountsAsync()
        {
            var userId = GetCurrentUserId();

            var accounts = await _db.BankAccounts
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.Active == 1)
                .Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    bank = a.Bank.ToString(),
                    country = a.Country.ToString(),
                    balance = a.Balance,
                    currency = a.Currency.Symbol,
                    currencyCode = a.Currency.Code.ToString()
                })
                .ToListAsync();

            return JsonSerializer.Serialize(accounts);
        }

        private Guid GetCurrentUserId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue("userId");
            return Guid.TryParse(claim, out var id) ? id
                : throw new InvalidOperationException("Authenticated user not found in context.");
        }
    }
}
