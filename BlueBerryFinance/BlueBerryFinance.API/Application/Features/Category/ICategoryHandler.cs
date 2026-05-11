using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Category
{
    public interface ICategoryHandler
    {
        Task<IReadOnlyList<CategoryViewModel>> GetAsync(CategoryFilterRequest filter);
        Task<CategoryViewModel> RegisterAsync(RegisterCategoryRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
