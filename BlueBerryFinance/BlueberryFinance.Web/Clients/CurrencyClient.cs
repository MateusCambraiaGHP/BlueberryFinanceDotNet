using BlueBerryFinance.Common.ViewModels;
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
            return await _http.GetFromJsonAsync<IReadOnlyList<CurrencyViewModel>>("api/v1.0/currency", ct);
        }
    }
}
