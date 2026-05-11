using BlueBerryFinance.API.Application.Features.Transaction;
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Transaction
{
    public interface ITransactionHandler
    {
        Task<PagedResult<TransactionViewModel>> ListAsync(ListTransactionsRequest request, Guid userId, CancellationToken ct = default);
        Task<TransactionViewModel?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
        Task<TransactionViewModel> RegisterAsync(RegisterTransactionRequest request, Guid userId, CancellationToken ct = default);
        Task<TransactionViewModel?> UpdateAsync(UpdateTransactionRequest request, Guid userId, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    }
}
