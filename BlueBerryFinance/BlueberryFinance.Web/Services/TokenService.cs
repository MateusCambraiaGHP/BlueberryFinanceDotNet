using BlueberryFinance.Web.Services.Interfaces;

namespace BlueberryFinance.Web.Services;

// Scoped per Blazor circuit.
// The API JWT is stored in the "api_jwt" HttpOnly cookie set by /account/login.
// We read it from the cookie on every new circuit (SSR prerender has HttpContext).
public class TokenService(IHttpContextAccessor httpContextAccessor) : ITokenService
{
    private const string CookieName = "api_jwt";
    private string? _token;

    public Task<string?> GetTokenAsync()
    {
        if (_token is not null) return Task.FromResult<string?>(_token);
        _token = httpContextAccessor.HttpContext?.Request.Cookies[CookieName];
        return Task.FromResult(_token);
    }

    public Task SetTokenAsync(string token)
    {
        _token = token;
        return Task.CompletedTask;
    }

    public void ClearToken() => _token = null;
}
