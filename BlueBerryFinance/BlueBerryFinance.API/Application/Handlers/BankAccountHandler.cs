using BlueBerryFinance.API.Application.Handlers.Interfaces;
using BlueBerryFinance.API.Application.Requests.BankAccount;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Handlers
{
    public class BankAccountHandler : IBankAccountHandler
    {
        private readonly AppDbContext _db;

        public BankAccountHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<BankAccountViewModel>> ListAsync(Guid userId, CancellationToken ct = default)
        {
            return await _db.BankAccounts
                .AsNoTracking()
                .Where(b => b.UserId == userId)
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
                .ToListAsync(ct);
        }

        public async Task<BankAccountViewModel?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            return await _db.BankAccounts
                .AsNoTracking()
                .Where(b => b.Id == id && b.UserId == userId)
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
                .FirstOrDefaultAsync(ct);
        }

        public async Task<BankAccountViewModel> RegisterAsync(RegisterBankAccountRequest request, Guid userId, CancellationToken ct = default)
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
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to register bank account.", ex);
            }

            return (await GetByIdAsync(entity.Id, userId, ct))!;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            var entity = await _db.BankAccounts
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, ct);

            if (entity is null) return false;

            entity.Delete();
            entity.SetLastModification(DateTime.UtcNow);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete bank account.", ex);
            }

            return true;
        }
    }
}
