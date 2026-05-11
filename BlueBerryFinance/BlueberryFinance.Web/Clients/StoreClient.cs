using BlueBerryFinance.Common.ViewModels;
using BlueberryFinance.Web.Models;
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
            var response = await _http.GetFromJsonAsync<ApiResponse<StoreViewModel>>("store", ct);
            return response?.Data?.AsReadOnly();
        }

        public async Task<StoreViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<StoreViewModel>>($"store/{id}", ct);
            return response?.Data?.FirstOrDefault();
        }

        public async Task<StoreViewModel?> CreateAsync(object request, CancellationToken ct = default)
        {
            var httpResponse = await _http.PostAsJsonAsync("store", request, ct);
            httpResponse.EnsureSuccessStatusCode();
            var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<StoreViewModel>>(ct);
            return response?.Data?.FirstOrDefault();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"store/{id}", ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
