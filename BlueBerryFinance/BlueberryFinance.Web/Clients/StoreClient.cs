using BlueBerryFinance.Common.ViewModels;
using System.Net.Http.Json;

namespace BlueberryFinance.Web.Clients
{
    public class StoreClient
    {
        private readonly HttpClient _http;

        public StoreClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<IReadOnlyList<StoreViewModel>?> GetAsync(CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<IReadOnlyList<StoreViewModel>>("api/v1.0/store", ct);
        }

        public async Task<StoreViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<StoreViewModel>($"api/v1.0/store/{id}", ct);
        }

        public async Task<StoreViewModel?> CreateAsync(object request, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync("api/v1.0/store", request, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StoreViewModel>(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"api/v1.0/store/{id}", ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
