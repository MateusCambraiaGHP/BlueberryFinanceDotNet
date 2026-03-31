using System.Net.Http.Json;

namespace BlueberryFinance.Web.Clients;

public record LoginResponse(string Token, string Email, string Name, string Profile, DateTime ExpiresAt);

public class AuthClient(HttpClient http)
{
    public async Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/v1.0/auth/login",
            new { Email = email, Password = password }, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LoginResponse>(ct);
    }
}
