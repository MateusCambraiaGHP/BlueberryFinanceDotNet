using BlueBerryFinance.Common.ViewModels;
using BlueberryFinance.Web.Models;
using System.Net.Http.Json;

namespace BlueberryFinance.Web.Clients
{
    public class BankAccountClient
    {
        private readonly HttpClient _http;

        public BankAccountClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<IReadOnlyList<BankAccountViewModel>?> GetAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<BankAccountViewModel>>("bank-account", ct);
            return response?.Data?.AsReadOnly();
        }

        public async Task<BankAccountViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<BankAccountViewModel>>($"bank-account/{id}", ct);
            return response?.Data?.FirstOrDefault();
        }

        public async Task<BankAccountViewModel?> CreateAsync(object request, CancellationToken ct = default)
        {
            var httpResponse = await _http.PostAsJsonAsync("bank-account", request, ct);
            httpResponse.EnsureSuccessStatusCode();
            var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<BankAccountViewModel>>(ct);
            return response?.Data?.FirstOrDefault();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"bank-account/{id}", ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
