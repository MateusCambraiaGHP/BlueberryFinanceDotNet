using BlueBerryFinance.API.Application.Features.Category;
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Category
{
    public interface ICategoryHandler
    {
        Task<IReadOnlyList<CategoryViewModel>> ListAsync(CancellationToken ct = default);
        Task<CategoryViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<CategoryViewModel> RegisterAsync(RegisterCategoryRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
