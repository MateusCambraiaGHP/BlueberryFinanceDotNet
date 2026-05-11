using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Features.BankAccount
{
    public class BankAccountHandler : IBankAccountHandler
    {
        private readonly AppDbContext _db;

        public BankAccountHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<BankAccountViewModel>> GetAsync(
            BankAccountFilterRequest filter, Guid userId)
        {
            var query = _db.BankAccounts
                .AsNoTracking()
                .Where(b => b.UserId == userId);

            if (filter.Id.HasValue)
                query = query.Where(b => b.Id == filter.Id.Value);

            return await query
                .Select(b => new BankAccountViewModel
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    Name = b.Name,
                    Bank = b.Bank.ToString(),
                    Country = b.Country.ToString(),
                    CurrencyCode = b.Currency.Code.ToString(),
                    CurrencySymbol = b.Currency.Symbol,
                    Balance = b.Balance,
                    Active = b.Active == 1
                })
                .ToListAsync();
        }

        public async Task<BankAccountViewModel> RegisterAsync(RegisterBankAccountRequest request, Guid userId)
        {
            var entity = new Data.Entities.BankAccount
            {
                UserId = userId,
                CurrencyId = request.CurrencyId,
                Name = request.Name,
                Bank = request.Bank,
                Country = request.Country,
                Balance = request.InitialBalance,
                Active = 1
            };

            entity.SetInsertionDate(DateTime.UtcNow);
            entity.SetLastModification(DateTime.UtcNow);

            _db.BankAccounts.Add(entity);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to register bank account.", ex);
            }

            return (await GetAsync(new BankAccountFilterRequest { Id = entity.Id }, userId)).First();
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var entity = await _db.BankAccounts
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (entity is null) return false;

            entity.Delete();
            entity.SetLastModification(DateTime.UtcNow);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete bank account.", ex);
            }

            return true;
        }
    }
}
