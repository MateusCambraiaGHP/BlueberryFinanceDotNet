using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.Common.ViewModels;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BlueBerryFinance.API.Application.Features.Transactions
{
    public interface ITransactionHandler
    {
        Task<BaseResponse<TransactionViewModel>> GetAsync(
            GetTransactionsFilterRequest request, 
            Guid userId);
        Task<BaseResponse<TransactionViewModel>> CreateAsync(
            RegisterTransactionRequest request, 
            Guid userId);
        Task<BaseResponse<TransactionViewModel>> UpdateAsync(
            UpdateTransactionRequest request, 
            Guid userId);
        Task<BaseResponse<TransactionViewModel>> DeleteAsync(
            Guid id, 
            Guid userId);
    }
}
