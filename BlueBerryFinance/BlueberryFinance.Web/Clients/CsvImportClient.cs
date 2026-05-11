using BlueBerryFinance.Common.ViewModels;
using System.Net.Http.Json;

namespace BlueberryFinance.Web.Clients
{
    public class CsvImportClient
    {
        private readonly HttpClient _http;

        public CsvImportClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<CsvImportResultViewModel?> ImportAsync(
            Guid bankAccountId,
            Stream fileStream,
            string fileName,
            CancellationToken ct = default)
        {
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
            content.Add(fileContent, "file", fileName);

            var response = await _http.PostAsync($"bank-import/{bankAccountId}", content, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CsvImportResultViewModel>(ct);
        }
    }
}
