using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Currency
{
    public interface ICurrencyHandler
    {
        Task<IReadOnlyList<CurrencyViewModel>> ListAsync(CancellationToken ct = default);
    }
}
