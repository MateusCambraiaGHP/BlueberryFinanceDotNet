using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.BankAccount
{
    public interface IBankAccountHandler
    {
        Task<IReadOnlyList<BankAccountViewModel>> GetAsync(
            BankAccountFilterRequest filter, 
            Guid userId);
        Task<BankAccountViewModel> RegisterAsync(
            RegisterBankAccountRequest request, 
            Guid userId);
        Task<bool> DeleteAsync(
            Guid id, 
            Guid userId);
    }
}
