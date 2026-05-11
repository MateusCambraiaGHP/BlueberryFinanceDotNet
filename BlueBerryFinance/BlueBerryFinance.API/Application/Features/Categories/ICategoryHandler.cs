using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Categories
{
    public interface ICategoryHandler
    {
        Task<BaseResponse<CategoryViewModel>> GetAsync(CategoryFilterRequest filter);
        Task<BaseResponse<CategoryViewModel>> CreateAsync(RegisterCategoryRequest request);
        Task<BaseResponse<CategoryViewModel>> DeleteAsync(Guid id);
    }
}
