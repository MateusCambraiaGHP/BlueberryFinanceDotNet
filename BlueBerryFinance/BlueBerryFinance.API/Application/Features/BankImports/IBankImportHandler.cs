using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.BankImports
{
    public interface IBankImportHandler
    {
        Task<CsvImportResultViewModel> ImportAsync(
            Guid bankAccountId,
            Guid userId,
            Stream csvStream);
    }
}
