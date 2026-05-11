using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.FixedExpenses
{
    public interface IFixedExpenseHandler
    {
        Task<BaseResponse<FixedExpenseViewModel>> GetAsync(
            FixedExpenseFilterRequest filter, 
            Guid userId);
        Task<BaseResponse<FixedExpenseViewModel>> CreateAsync(
            RegisterFixedExpenseRequest request, 
            Guid userId);
        Task<BaseResponse<FixedExpenseViewModel>> DeleteAsync(
            Guid id, 
            Guid userId);
    }
}
