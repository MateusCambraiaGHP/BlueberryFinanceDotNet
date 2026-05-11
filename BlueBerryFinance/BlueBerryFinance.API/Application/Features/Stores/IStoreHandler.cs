using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Stores
{
    public interface IStoreHandler
    {
        Task<BaseResponse<StoreViewModel>> GetAsync(StoreFilterRequest filter);
        Task<BaseResponse<StoreViewModel>> CreateAsync(RegisterStoreRequest request);
        Task<BaseResponse<StoreViewModel>> DeleteAsync(Guid id);
    }
}
