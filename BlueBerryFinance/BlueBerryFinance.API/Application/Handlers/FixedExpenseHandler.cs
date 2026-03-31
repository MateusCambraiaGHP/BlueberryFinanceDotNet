using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Application.Requests.FixedExpense;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Handlers
{
    public class FixedExpenseHandler : IFixedExpenseHandler
    {
        private readonly AppDbContext _db;

        public FixedExpenseHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<FixedExpenseViewModel>> ListAsync(Guid userId, CancellationToken ct = default)
        {
            return await _db.FixedExpenses
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .Select(f => new FixedExpenseViewModel
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    Name = f.Name,
                    Description = f.Description,
                    Amount = f.Amount,
                    CurrencyCode = f.Currency.Code.ToString(),
                    CurrencySymbol = f.Currency.Symbol,
                    DayOfMonth = f.DayOfMonth,
                    IsRecurring = f.IsRecurring,
                    StoreName = f.Store.Name,
                    Active = f.Active == 1
                })
                .ToListAsync(ct);
        }

        public async Task<FixedExpenseViewModel?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            return await _db.FixedExpenses
                .AsNoTracking()
                .Where(f => f.Id == id && f.UserId == userId)
                .Select(f => new FixedExpenseViewModel
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    Name = f.Name,
                    Description = f.Description,
                    Amount = f.Amount,
                    CurrencyCode = f.Currency.Code.ToString(),
                    CurrencySymbol = f.Currency.Symbol,
                    DayOfMonth = f.DayOfMonth,
                    IsRecurring = f.IsRecurring,
                    StoreName = f.Store.Name,
                    Active = f.Active == 1
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<FixedExpenseViewModel> RegisterAsync(RegisterFixedExpenseRequest request, Guid userId, CancellationToken ct = default)
        {
            var entity = new Data.Entities.FixedExpense
            {
                UserId = userId,
                CurrencyId = request.CurrencyId,
                StoreId = request.StoreId,
                Name = request.Name,
                Description = request.Description,
                Amount = request.Amount,
                DayOfMonth = request.DayOfMonth,
                IsRecurring = request.IsRecurring,
                Active = 1
            };

            entity.SetInsertionDate(DateTime.UtcNow);
            entity.SetLastModification(DateTime.UtcNow);

            _db.FixedExpenses.Add(entity);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to register fixed expense.", ex);
            }

            return (await GetByIdAsync(entity.Id, userId, ct))!;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            var entity = await _db.FixedExpenses
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId, ct);

            if (entity is null) return false;

            entity.Delete();
            entity.SetLastModification(DateTime.UtcNow);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete fixed expense.", ex);
            }

            return true;
        }
    }
}
