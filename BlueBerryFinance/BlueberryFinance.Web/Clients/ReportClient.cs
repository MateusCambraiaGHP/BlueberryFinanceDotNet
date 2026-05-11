using BlueBerryFinance.Common.ViewModels;
using BlueberryFinance.Web.Models;
using System.Net.Http.Json;

namespace BlueberryFinance.Web.Clients
{
    public class ReportClient
    {
        private readonly HttpClient _http;

        public ReportClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<MonthlyReportViewModel?> GetMonthlyAsync(
            int year, int month, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<MonthlyReportViewModel>>(
                $"report/monthly?year={year}&month={month}", ct);
            return response?.Data?.FirstOrDefault();
        }
    }
}
