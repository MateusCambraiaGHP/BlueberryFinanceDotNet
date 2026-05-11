using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.FixedExpense
{
    public interface IFixedExpenseHandler
    {
        Task<IReadOnlyList<FixedExpenseViewModel>> GetAsync(FixedExpenseFilterRequest filter, Guid userId, CancellationToken ct = default);
        Task<FixedExpenseViewModel> RegisterAsync(RegisterFixedExpenseRequest request, Guid userId, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    }
}
