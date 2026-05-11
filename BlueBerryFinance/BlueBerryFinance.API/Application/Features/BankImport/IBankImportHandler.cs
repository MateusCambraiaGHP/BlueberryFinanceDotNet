using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.BankImport
{
    public interface IBankImportHandler
    {
        /// <summary>
        /// Parses a bank CSV export and saves new transactions to the database.
        /// Skips rows that already exist (same date + amount + description on the same account).
        /// </summary>
        Task<CsvImportResultViewModel> ImportAsync(
            Guid bankAccountId,
            Guid userId,
            Stream csvStream,
            CancellationToken ct = default);
    }
}
