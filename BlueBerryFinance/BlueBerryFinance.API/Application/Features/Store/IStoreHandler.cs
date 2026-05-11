using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Store
{
    public interface IStoreHandler
    {
        Task<IReadOnlyList<StoreViewModel>> GetAsync(StoreFilterRequest filter, CancellationToken ct = default);
        Task<StoreViewModel> RegisterAsync(RegisterStoreRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
