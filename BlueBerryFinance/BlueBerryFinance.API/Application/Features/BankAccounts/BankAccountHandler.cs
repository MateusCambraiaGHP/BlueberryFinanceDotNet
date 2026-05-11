using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;
using BlueBerryFinance.API.Domain.Entities;

namespace BlueBerryFinance.API.Application.Features.BankAccounts
{
    public class BankAccountHandler : IBankAccountHandler
    {
        private readonly AppDbContext _db;

        public BankAccountHandler(AppDbContext db)
            => _db = db;

        public async Task<BaseResponse<BankAccountViewModel>> GetAsync(
            BankAccountFilterRequest filter, Guid userId)
        {
            var query = _db.BankAccounts
                .AsNoTracking()
                .Where(ba => ba.UserId == userId);

            if (filter.Id.HasValue)
                query = query.Where(ba => ba.Id == filter.Id.Value);

            var entities = await query
                .Select(ba => ToViewModel(ba))
                .ToListAsync();

            return BaseResponse<BankAccountViewModel>.Ok(entities);
        }

        public async Task<BaseResponse<BankAccountViewModel>> CreateAsync(RegisterBankAccountRequest request, Guid userId)
        {
            var entity = new BankAccount
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
                throw new InvalidOperationException("Failed to create bank account.", ex);
            }

            var bankAccounts = await GetAsync(new BankAccountFilterRequest { Id = entity.Id }, userId);

            return bankAccounts;
        }

        public async Task<BaseResponse<BankAccountViewModel>> DeleteAsync(Guid id, Guid userId)
        {
            var entity = await _db.BankAccounts
                .FirstOrDefaultAsync(
                    ba => ba.Id == id &&
                    ba.UserId == userId);

            if (entity is null) 
                return BaseResponse<BankAccountViewModel>.Fail(new List<string> { "Bank account not found." });

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

            return BaseResponse<BankAccountViewModel>.Ok();
        }

        private static BankAccountViewModel ToViewModel(BankAccount b) => new BankAccountViewModel(
            b.Id,
            b.UserId,
            b.Name,
            b.Bank.ToString(),
            b.Country.ToString(),
            b.Currency.Code.ToString(),
            b.Currency.Symbol,
            b.Balance,
            b.Active == 1
            );
    }
}
