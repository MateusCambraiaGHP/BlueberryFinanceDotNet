using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Features.FixedExpense
{
    public class FixedExpenseHandler : IFixedExpenseHandler
    {
        private readonly AppDbContext _db;

        public FixedExpenseHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<FixedExpenseViewModel>> GetAsync(
            FixedExpenseFilterRequest filter, Guid userId)
        {
            var query = _db.FixedExpenses
                .AsNoTracking()
                .Where(f => f.UserId == userId);

            if (filter.Id.HasValue)
                query = query.Where(f => f.Id == filter.Id.Value);

            return await query
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
                .ToListAsync();
        }

        public async Task<FixedExpenseViewModel> RegisterAsync(RegisterFixedExpenseRequest request, Guid userId)
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
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to register fixed expense.", ex);
            }

            return (await GetAsync(new FixedExpenseFilterRequest { Id = entity.Id }, userId)).First();
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var entity = await _db.FixedExpenses
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (entity is null) return false;

            entity.Delete();
            entity.SetLastModification(DateTime.UtcNow);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete fixed expense.", ex);
            }

            return true;
        }
    }
}
