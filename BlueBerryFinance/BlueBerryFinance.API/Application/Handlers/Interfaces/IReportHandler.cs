using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Handlers.Interfaces
{
    public interface IReportHandler
    {
        Task<MonthlyReportViewModel> GetMonthlyAsync(int year, int month, Guid userId, CancellationToken ct = default);
    }
}
