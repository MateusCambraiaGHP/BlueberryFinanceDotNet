using BlueBerryFinance.Common.ViewModels;
using System.Net.Http.Json;

namespace BlueberryFinance.Web.Clients
{
    public class TransactionClient
    {
        private readonly HttpClient _http;

        public TransactionClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<PagedResult<TransactionViewModel>?> GetAsync(
            int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<PagedResult<TransactionViewModel>>(
                $"api/v1.0/transaction?page={page}&pageSize={pageSize}", ct);
        }

        public async Task<TransactionViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<TransactionViewModel>($"api/v1.0/transaction/{id}", ct);
        }

        public async Task<TransactionViewModel?> CreateAsync(object request, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync("api/v1.0/transaction", request, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TransactionViewModel>(ct);
        }

        public async Task<TransactionViewModel?> UpdateAsync(Guid id, object request, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"api/v1.0/transaction/{id}", request, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TransactionViewModel>(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"api/v1.0/transaction/{id}", ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
