using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Reports
{
    public interface IReportHandler
    {
        Task<BaseResponse<MonthlyReportViewModel>> GetMonthlyAsync(int year, int month, Guid userId);
    }
}
