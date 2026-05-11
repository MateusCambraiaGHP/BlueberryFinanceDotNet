using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Report
{
    public interface IReportHandler
    {
        Task<MonthlyReportViewModel> GetMonthlyAsync(int year, int month, Guid userId);
    }
}
