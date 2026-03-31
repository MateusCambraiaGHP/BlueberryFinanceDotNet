using BlueBerryFinance.API.Application.Requests.FixedExpense;
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Handlers.Interfaces
{
    public interface IFixedExpenseHandler
    {
        Task<IReadOnlyList<FixedExpenseViewModel>> ListAsync(Guid userId, CancellationToken ct = default);
        Task<FixedExpenseViewModel?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
        Task<FixedExpenseViewModel> RegisterAsync(RegisterFixedExpenseRequest request, Guid userId, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    }
}
