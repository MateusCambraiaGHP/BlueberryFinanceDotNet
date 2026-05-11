using BlueBerryFinance.Common.ViewModels;
using BlueberryFinance.Web.Models;
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
            var response = await _http.GetFromJsonAsync<ApiResponse<AgentApprovalViewModel>>(
                "agent-approvals/pending", ct);
            return response?.Data?.AsReadOnly();
        }

        public async Task<AgentApprovalViewModel?> ApproveAsync(Guid id, CancellationToken ct = default)
        {
            var httpResponse = await _http.PostAsync($"agent-approvals/{id}/approve", null, ct);
            await EnsureSuccessAsync(httpResponse, ct);
            var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<AgentApprovalViewModel>>(ct);
            return response?.Data?.FirstOrDefault();
        }

        public async Task<AgentApprovalViewModel?> RejectAsync(Guid id, CancellationToken ct = default)
        {
            var httpResponse = await _http.PostAsync($"agent-approvals/{id}/reject", null, ct);
            await EnsureSuccessAsync(httpResponse, ct);
            var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<AgentApprovalViewModel>>(ct);
            return response?.Data?.FirstOrDefault();
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
