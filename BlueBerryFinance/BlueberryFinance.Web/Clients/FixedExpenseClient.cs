using BlueBerryFinance.Common.ViewModels;
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
            return await _http.GetFromJsonAsync<IReadOnlyList<FixedExpenseViewModel>>("api/v1.0/fixed-expense", ct);
        }

        public async Task<FixedExpenseViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<FixedExpenseViewModel>($"api/v1.0/fixed-expense/{id}", ct);
        }

        public async Task<FixedExpenseViewModel?> CreateAsync(object request, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync("api/v1.0/fixed-expense", request, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<FixedExpenseViewModel>(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"api/v1.0/fixed-expense/{id}", ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
