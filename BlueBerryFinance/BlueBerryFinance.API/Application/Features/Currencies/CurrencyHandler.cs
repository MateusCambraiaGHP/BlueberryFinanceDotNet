using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.API.Data.Context;
using BlueBerryFinance.API.Domain.Entities;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlueBerryFinance.API.Application.Features.Currencies
{
    public class CurrencyHandler : ICurrencyHandler
    {
        private readonly AppDbContext _db;

        public CurrencyHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<BaseResponse<CurrencyViewModel>> GetAsync()
        {
            var currencies = await _db.Currencies
                .AsNoTracking()
                .Select(c => ToViewModel(c))
                .ToListAsync();

            return BaseResponse<CurrencyViewModel>.Ok(currencies);
        }

        private static CurrencyViewModel ToViewModel(Currency c) => new CurrencyViewModel(
            c.Id,
            c.Code.ToString(),
            c.Symbol,
            c.Name);
    }
}
