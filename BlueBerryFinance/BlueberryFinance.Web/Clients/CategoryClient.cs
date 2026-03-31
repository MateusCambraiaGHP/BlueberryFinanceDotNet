using BlueBerryFinance.Common.ViewModels;
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
            return await _http.GetFromJsonAsync<IReadOnlyList<CategoryViewModel>>("api/v1.0/category", ct);
        }

        public async Task<CategoryViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<CategoryViewModel>($"api/v1.0/category/{id}", ct);
        }

        public async Task<CategoryViewModel?> CreateAsync(object request, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync("api/v1.0/category", request, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CategoryViewModel>(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"api/v1.0/category/{id}", ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
