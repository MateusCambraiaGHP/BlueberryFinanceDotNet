using BlueBerryFinance.API.Application.Features.BankAccount;
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.BankAccount
{
    public interface IBankAccountHandler
    {
        Task<IReadOnlyList<BankAccountViewModel>> ListAsync(Guid userId, CancellationToken ct = default);
        Task<BankAccountViewModel?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
        Task<BankAccountViewModel> RegisterAsync(RegisterBankAccountRequest request, Guid userId, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    }
}
