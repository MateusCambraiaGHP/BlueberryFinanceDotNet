using BlueBerryFinance.Common.ViewModels;
using System.Net.Http.Json;

namespace BlueberryFinance.Web.Clients
{
    public class AgentApprovalClient
    {
        private readonly HttpClient _http;

        public AgentApprovalClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<IReadOnlyList<AgentApprovalViewModel>?> GetPendingAsync(CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<IReadOnlyList<AgentApprovalViewModel>>(
                "api/v1.0/agent-approvals/pending", ct);
        }

        public async Task<AgentApprovalViewModel?> ApproveAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.PostAsync($"api/v1.0/agent-approvals/{id}/approve", null, ct);
            await EnsureSuccessAsync(response, ct);
            return await response.Content.ReadFromJsonAsync<AgentApprovalViewModel>(ct);
        }

        public async Task<AgentApprovalViewModel?> RejectAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.PostAsync($"api/v1.0/agent-approvals/{id}/reject", null, ct);
            await EnsureSuccessAsync(response, ct);
            return await response.Content.ReadFromJsonAsync<AgentApprovalViewModel>(ct);
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
        {
            if (response.IsSuccessStatusCode) return;
            var body = await response.Content.ReadAsStringAsync(ct);
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                    throw new HttpRequestException(msg.GetString());
            }
            catch (System.Text.Json.JsonException) { }
            throw new HttpRequestException($"{(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }
    }
}
