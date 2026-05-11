using BlueBerryFinance.Common.ViewModels;
using BlueberryFinance.Web.Models;
using System.Net.Http.Json;

namespace BlueberryFinance.Web.Clients
{
    public class CurrencyClient
    {
        private readonly HttpClient _http;

        public CurrencyClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<IReadOnlyList<CurrencyViewModel>?> GetAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CurrencyViewModel>>("currency", ct);
            return response?.Data?.AsReadOnly();
        }
    }
}
