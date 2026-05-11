using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Features.Transaction
{
    public class TransactionHandler : ITransactionHandler
    {
        private readonly AppDbContext _db;

        public TransactionHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<TransactionViewModel>> GetAsync(
            ListTransactionsRequest request, Guid userId, CancellationToken ct = default)
        {
            var query = _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId);

            if (request.Id.HasValue)
                query = query.Where(t => t.Id == request.Id.Value);

            if (request.BankAccountId.HasValue)
                query = query.Where(t => t.BankAccountId == request.BankAccountId.Value);

            if (request.CategoryId.HasValue)
                query = query.Where(t => t.CategoryId == request.CategoryId.Value);

            if (request.DateFrom.HasValue)
                query = query.Where(t => t.TransactionDate >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
                query = query.Where(t => t.TransactionDate <= request.DateTo.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(t => t.Description.Contains(request.Search) || t.Store.Name.Contains(request.Search));

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new TransactionViewModel
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    BankAccountId = t.BankAccountId,
                    BankAccountName = t.BankAccount.Name,
                    StoreId = t.StoreId,
                    StoreName = t.Store.Name,
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category.Name,
                    CategoryColor = t.Category.Color,
                    CurrencyCode = t.Currency.Code.ToString(),
                    CurrencySymbol = t.Currency.Symbol,
                    TransactionType = t.TransactionType.ToString(),
                    Source = t.Source.ToString(),
                    Amount = t.Amount,
                    Description = t.Description,
                    TransactionDate = t.TransactionDate,
                    ImageUrl = t.ImageUrl,
                    CorrelationId = t.CorrelationId,
                    InsertionDate = t.InsertionDate
                })
                .ToListAsync(ct);

            return new PagedResult<TransactionViewModel>
            {
                Items = items,
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task<TransactionViewModel> RegisterAsync(
            RegisterTransactionRequest request, Guid userId, CancellationToken ct = default)
        {
            var account = await _db.BankAccounts
                .FirstOrDefaultAsync(a => a.Id == request.BankAccountId && a.UserId == userId, ct)
                ?? throw new InvalidOperationException("Bank account not found.");

            var entity = new Data.Entities.Transaction
            {
                UserId = userId,
                BankAccountId = request.BankAccountId,
                StoreId = request.StoreId,
                CategoryId = request.CategoryId,
                CurrencyId = request.CurrencyId,
                FixedExpenseId = request.FixedExpenseId,
                OriginType = request.OriginType,
                TransactionType = request.TransactionType,
                Source = request.Source,
                Amount = request.Amount,
                Description = request.Description,
                TransactionDate = request.TransactionDate,
                ImageUrl = request.ImageUrl,
                Active = 1
            };

            entity.SetInsertionDate(DateTime.UtcNow);
            entity.SetLastModification(DateTime.UtcNow);

            account.Balance += request.TransactionType == Data.Entities.Enums.TransactionType.Income
                ? request.Amount
                : -request.Amount;

            _db.Transactions.Add(entity);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to register transaction.", ex);
            }

            return (await GetAsync(new ListTransactionsRequest { Id = entity.Id }, userId, ct)).Items.First();
        }

        public async Task<TransactionViewModel?> UpdateAsync(
            UpdateTransactionRequest request, Guid userId, CancellationToken ct = default)
        {
            var entity = await _db.Transactions
                .FirstOrDefaultAsync(t => t.Id == request.Id && t.UserId == userId, ct);

            if (entity is null) return null;

            var account = await _db.BankAccounts
                .FirstOrDefaultAsync(a => a.Id == entity.BankAccountId && a.UserId == userId, ct)
                ?? throw new InvalidOperationException("Bank account not found.");

            account.Balance -= entity.TransactionType == Data.Entities.Enums.TransactionType.Income
                ? entity.Amount
                : -entity.Amount;

            entity.StoreId = request.StoreId;
            entity.CategoryId = request.CategoryId;
            entity.OriginType = request.OriginType;
            entity.TransactionType = request.TransactionType;
            entity.Amount = request.Amount;
            entity.Description = request.Description;
            entity.TransactionDate = request.TransactionDate;
            entity.SetLastModification(DateTime.UtcNow);

            account.Balance += request.TransactionType == Data.Entities.Enums.TransactionType.Income
                ? request.Amount
                : -request.Amount;

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to update transaction.", ex);
            }

            return (await GetAsync(new ListTransactionsRequest { Id = entity.Id }, userId, ct)).Items.FirstOrDefault();
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            var entity = await _db.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);

            if (entity is null) return false;

            var account = await _db.BankAccounts
                .FirstOrDefaultAsync(a => a.Id == entity.BankAccountId && a.UserId == userId, ct);

            if (account is not null)
            {
                account.Balance -= entity.TransactionType == Data.Entities.Enums.TransactionType.Income
                    ? entity.Amount
                    : -entity.Amount;
            }

            entity.Delete();
            entity.SetLastModification(DateTime.UtcNow);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete transaction.", ex);
            }

            return true;
        }
    }
}
