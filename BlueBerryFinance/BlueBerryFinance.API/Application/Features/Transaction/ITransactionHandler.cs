using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Transaction
{
    public interface ITransactionHandler
    {
        Task<PagedResult<TransactionViewModel>> GetAsync(
            ListTransactionsRequest request, 
            Guid userId);
        Task<TransactionViewModel> RegisterAsync(
            RegisterTransactionRequest request, 
            Guid userId);
        Task<TransactionViewModel?> UpdateAsync(
            UpdateTransactionRequest request, 
            Guid userId);
        Task<bool> DeleteAsync(
            Guid id, 
            Guid userId);
    }
}
