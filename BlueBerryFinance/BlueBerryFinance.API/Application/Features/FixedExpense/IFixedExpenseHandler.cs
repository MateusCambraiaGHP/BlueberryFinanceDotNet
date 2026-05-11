using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.FixedExpense
{
    public interface IFixedExpenseHandler
    {
        Task<IReadOnlyList<FixedExpenseViewModel>> GetAsync(
            FixedExpenseFilterRequest filter, 
            Guid userId);
        Task<FixedExpenseViewModel> RegisterAsync(
            RegisterFixedExpenseRequest request, 
            Guid userId);
        Task<bool> DeleteAsync(
            Guid id, 
            Guid userId);
    }
}
