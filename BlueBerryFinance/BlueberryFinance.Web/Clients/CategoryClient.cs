using BlueBerryFinance.Common.ViewModels;
using BlueberryFinance.Web.Models;
using System.Net.Http.Json;

namespace BlueberryFinance.Web.Clients
{
    public class CategoryClient
    {
        private readonly HttpClient _http;

        public CategoryClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<IReadOnlyList<CategoryViewModel>?> GetAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CategoryViewModel>>("category", ct);
            return response?.Data?.AsReadOnly();
        }

        public async Task<CategoryViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CategoryViewModel>>($"category/{id}", ct);
            return response?.Data?.FirstOrDefault();
        }

        public async Task<CategoryViewModel?> CreateAsync(object request, CancellationToken ct = default)
        {
            var httpResponse = await _http.PostAsJsonAsync("category", request, ct);
            httpResponse.EnsureSuccessStatusCode();
            var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryViewModel>>(ct);
            return response?.Data?.FirstOrDefault();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"category/{id}", ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
