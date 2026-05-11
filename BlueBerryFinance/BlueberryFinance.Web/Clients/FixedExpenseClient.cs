using BlueBerryFinance.Common.ViewModels;
using BlueberryFinance.Web.Models;
using System.Net.Http.Json;

namespace BlueberryFinance.Web.Clients
{
    public class FixedExpenseClient
    {
        private readonly HttpClient _http;

        public FixedExpenseClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<IReadOnlyList<FixedExpenseViewModel>?> GetAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<FixedExpenseViewModel>>("fixed-expense", ct);
            return response?.Data?.AsReadOnly();
        }

        public async Task<FixedExpenseViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<FixedExpenseViewModel>>($"fixed-expense/{id}", ct);
            return response?.Data?.FirstOrDefault();
        }

        public async Task<FixedExpenseViewModel?> CreateAsync(object request, CancellationToken ct = default)
        {
            var httpResponse = await _http.PostAsJsonAsync("fixed-expense", request, ct);
            httpResponse.EnsureSuccessStatusCode();
            var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<FixedExpenseViewModel>>(ct);
            return response?.Data?.FirstOrDefault();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"fixed-expense/{id}", ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
