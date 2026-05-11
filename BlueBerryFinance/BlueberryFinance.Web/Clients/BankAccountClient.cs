using BlueBerryFinance.Common.ViewModels;
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
            return await _http.GetFromJsonAsync<IReadOnlyList<BankAccountViewModel>>("api/v1.0/bank-account", ct);
        }

        public async Task<BankAccountViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<BankAccountViewModel>($"api/v1.0/bank-account/{id}", ct);
        }

        public async Task<BankAccountViewModel?> CreateAsync(object request, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync("api/v1.0/bank-account", request, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BankAccountViewModel>(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"api/v1.0/bank-account/{id}", ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
