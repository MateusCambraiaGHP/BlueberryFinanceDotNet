using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Handlers.Interfaces
{
    public interface ICurrencyHandler
    {
        Task<IReadOnlyList<CurrencyViewModel>> ListAsync(CancellationToken ct = default);
    }
}
