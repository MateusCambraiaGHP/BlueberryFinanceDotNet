using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.BankAccounts
{
    public interface IBankAccountHandler
    {
        Task<BaseResponse<BankAccountViewModel>> GetAsync(
            BankAccountFilterRequest filter, 
            Guid userId);
        Task<BaseResponse<BankAccountViewModel>> CreateAsync(
            RegisterBankAccountRequest request, 
            Guid userId);
        Task<BaseResponse<BankAccountViewModel>> DeleteAsync(
            Guid id, 
            Guid userId);
    }
}
