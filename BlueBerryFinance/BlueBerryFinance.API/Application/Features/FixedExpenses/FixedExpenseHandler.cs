using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Domain.Entities;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Features.FixedExpenses
{
    public class FixedExpenseHandler : IFixedExpenseHandler
    {
        private readonly AppDbContext _db;

        public FixedExpenseHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<BaseResponse<FixedExpenseViewModel>> GetAsync(FixedExpenseFilterRequest filter, Guid userId)
        {
            var query = _db.FixedExpenses
                .AsNoTracking()
                .Where(f => f.UserId == userId);

            if (filter.Id.HasValue)
                query = query.Where(f => f.Id == filter.Id.Value);

            var entities = await query
                .Select(f => ToViewModel(f))
                .ToListAsync();

            return BaseResponse<FixedExpenseViewModel>.Ok(entities);
        }

        public async Task<BaseResponse<FixedExpenseViewModel>> CreateAsync(RegisterFixedExpenseRequest request, Guid userId)
        {
            var entity = new FixedExpense
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

            var response = await GetAsync(new FixedExpenseFilterRequest { Id = entity.Id }, userId);

            return response;
        }

        public async Task<BaseResponse<FixedExpenseViewModel>> DeleteAsync(Guid id, Guid userId)
        {
            var entity = await _db.FixedExpenses
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (entity is null) return BaseResponse<FixedExpenseViewModel>.Fail(new List<string> { "Fixed expense not found." });

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

            return BaseResponse<FixedExpenseViewModel>.Ok();
        }

        private static FixedExpenseViewModel ToViewModel(FixedExpense f) => new FixedExpenseViewModel(
            f.Id,
            f.UserId,
            f.Name,
            f.Description,
            f.Amount,
            f.Currency.Code.ToString(),
            f.Currency.Symbol,
            f.DayOfMonth,
            f.IsRecurring,
            f.Store.Name,
            f.Active == 1);
    }
}
