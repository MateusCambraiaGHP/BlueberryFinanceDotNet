namespace BlueberryFinance.Web.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string?> GetTokenAsync();
        Task SetTokenAsync(string token);
        void ClearToken();
    }
}
