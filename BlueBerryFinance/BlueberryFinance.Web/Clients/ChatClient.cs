using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BlueberryFinance.Web.Clients
{
    public class ChatClient
    {
        private readonly HttpClient _http;

        public ChatClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<string?> UploadImageAsync(Stream imageStream, string fileName, string contentType, CancellationToken ct = default)
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(imageStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(streamContent, "file", fileName);

            var response = await _http.PostAsync("api/v1.0/chat/upload-image", content, ct);
            if (!response.IsSuccessStatusCode) return null;

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            return doc.RootElement.TryGetProperty("url", out var url) ? url.GetString() : null;
        }

        public async IAsyncEnumerable<string> StreamAsync(
            string prompt,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/v1.0/chat/stream");
            request.Content = JsonContent.Create(new { prompt });

            var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(ct);
                if (line is null) break;
                if (!line.StartsWith("data: ")) continue;

                var data = line[6..];
                if (data == "[DONE]") break;

                string? text = null;
                try
                {
                    using var doc = JsonDocument.Parse(data);
                    if (doc.RootElement.TryGetProperty("content", out var content))
                        text = content.GetString();
                }
                catch { /* skip malformed events */ }

                if (!string.IsNullOrEmpty(text))
                    yield return text;
            }
        }
    }
}
