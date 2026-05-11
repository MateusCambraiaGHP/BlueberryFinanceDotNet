using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.BankImport
{
    public interface IBankImportHandler
    {
        Task<CsvImportResultViewModel> ImportAsync(
            Guid bankAccountId,
            Guid userId,
            Stream csvStream);
    }
}
