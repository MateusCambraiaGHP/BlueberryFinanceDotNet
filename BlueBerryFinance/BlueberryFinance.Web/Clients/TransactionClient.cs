using BlueBerryFinance.Common.ViewModels;
using BlueberryFinance.Web.Models;
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
            var response = await _http.GetFromJsonAsync<ApiResponse<TransactionViewModel>>(
                $"transaction?page={page}&pageSize={pageSize}", ct);

            if (response?.Data is null || response.Pagination is null)
                return null;

            return new PagedResult<TransactionViewModel>
            {
                Items = response.Data.AsReadOnly(),
                TotalCount = response.Pagination.TotalCount,
                Page = response.Pagination.Page,
                PageSize = response.Pagination.PageSize
            };
        }

        public async Task<TransactionViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<TransactionViewModel>>($"transaction/{id}", ct);
            return response?.Data?.FirstOrDefault();
        }

        public async Task<TransactionViewModel?> CreateAsync(object request, CancellationToken ct = default)
        {
            var httpResponse = await _http.PostAsJsonAsync("transaction", request, ct);
            httpResponse.EnsureSuccessStatusCode();
            var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<TransactionViewModel>>(ct);
            return response?.Data?.FirstOrDefault();
        }

        public async Task<TransactionViewModel?> UpdateAsync(Guid id, object request, CancellationToken ct = default)
        {
            var httpResponse = await _http.PutAsJsonAsync($"transaction/{id}", request, ct);
            httpResponse.EnsureSuccessStatusCode();
            var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<TransactionViewModel>>(ct);
            return response?.Data?.FirstOrDefault();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"transaction/{id}", ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
