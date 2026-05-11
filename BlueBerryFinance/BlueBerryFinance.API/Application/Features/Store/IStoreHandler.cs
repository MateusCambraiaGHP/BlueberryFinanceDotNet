using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Store
{
    public interface IStoreHandler
    {
        Task<IReadOnlyList<StoreViewModel>> GetAsync(StoreFilterRequest filter);
        Task<StoreViewModel> RegisterAsync(RegisterStoreRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
