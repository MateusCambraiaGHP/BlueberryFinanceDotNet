using BlueBerryFinance.API.Application.Features.Store;
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Store
{
    public interface IStoreHandler
    {
        Task<IReadOnlyList<StoreViewModel>> ListAsync(CancellationToken ct = default);
        Task<StoreViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<StoreViewModel> RegisterAsync(RegisterStoreRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
