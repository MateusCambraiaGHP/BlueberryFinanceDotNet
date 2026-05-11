using BlueBerryFinance.API.Application.Features.Currency;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Features.Currency
{
    public class CurrencyHandler : ICurrencyHandler
    {
        private readonly AppDbContext _db;

        public CurrencyHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<CurrencyViewModel>> GetAsync()
        {
            return await _db.Currencies
                .AsNoTracking()
                .Select(c => new CurrencyViewModel
                {
                    Id = c.Id,
                    Code = c.Code.ToString(),
                    Symbol = c.Symbol,
                    Name = c.Name
                })
                .ToListAsync();
        }
    }
}
