using BlueBerryFinance.Common.ViewModels;
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
            return await _http.GetFromJsonAsync<MonthlyReportViewModel>(
                $"api/v1.0/report/monthly?year={year}&month={month}", ct);
        }
    }
}
