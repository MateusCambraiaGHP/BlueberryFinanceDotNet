using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Transactions
{
    public interface ITransactionHandler
    {
        Task<PagedResult<TransactionViewModel>> GetAsync(
            ListTransactionsRequest request, 
            Guid userId);
        Task<TransactionViewModel> CreateAsync(
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
