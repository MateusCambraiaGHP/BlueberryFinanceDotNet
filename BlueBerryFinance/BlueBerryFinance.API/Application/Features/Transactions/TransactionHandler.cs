using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Domain.Entities;
using BlueBerryFinance.API.Domain.Entities.Enums;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Features.Transactions
{
    public class TransactionHandler : ITransactionHandler
    {
        private readonly AppDbContext _db;

        public TransactionHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<TransactionViewModel>> GetAsync(
            ListTransactionsRequest request, Guid userId)
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

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => ToViewModel(t))
                .ToListAsync();

            return new PagedResult<TransactionViewModel>
            {
                Items = items,
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task<TransactionViewModel> CreateAsync(
            RegisterTransactionRequest request, Guid userId)
        {
            var account = await _db.BankAccounts
                .FirstOrDefaultAsync(a => a.Id == request.BankAccountId && a.UserId == userId)
                ?? throw new InvalidOperationException("Bank account not found.");

            var entity = new Transaction
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

            account.Balance += request.TransactionType == TransactionType.Income
                ? request.Amount
                : -request.Amount;

            _db.Transactions.Add(entity);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to register transaction.", ex);
            }

            return (await GetAsync(new ListTransactionsRequest { Id = entity.Id }, userId)).Items.First();
        }

        public async Task<TransactionViewModel?> UpdateAsync(
            UpdateTransactionRequest request, Guid userId)
        {
            var entity = await _db.Transactions
                .FirstOrDefaultAsync(t => t.Id == request.Id && t.UserId == userId);

            if (entity is null) return null;

            var account = await _db.BankAccounts
                .FirstOrDefaultAsync(a => a.Id == entity.BankAccountId && a.UserId == userId)
                ?? throw new InvalidOperationException("Bank account not found.");

            account.Balance -= entity.TransactionType == TransactionType.Income
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

            account.Balance += request.TransactionType == TransactionType.Income
                ? request.Amount
                : -request.Amount;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to update transaction.", ex);
            }

            return (await GetAsync(new ListTransactionsRequest { Id = entity.Id }, userId)).Items.FirstOrDefault();
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var entity = await _db.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (entity is null) return false;

            var account = await _db.BankAccounts
                .FirstOrDefaultAsync(a => a.Id == entity.BankAccountId && a.UserId == userId);

            if (account is not null)
            {
                account.Balance -= entity.TransactionType == TransactionType.Income
                    ? entity.Amount
                    : -entity.Amount;
            }

            entity.Delete();
            entity.SetLastModification(DateTime.UtcNow);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete transaction.", ex);
            }

            return true;
        }

        private static TransactionViewModel ToViewModel(Transaction t) => new(
            t.Id,
            t.UserId,
            t.BankAccountId,
            t.BankAccount.Name,
            t.StoreId,
            t.Store.Name,
            t.CategoryId,
            t.Category.Name,
            t.Category.Color ?? string.Empty,
            t.Currency.Code.ToString(),
            t.Currency.Symbol,
            t.TransactionType.ToString(),
            t.Source.ToString(),
            t.Amount,
            t.Description,
            t.TransactionDate,
            t.ImageUrl,
            t.CorrelationId,
            t.InsertionDate);
    }
}
