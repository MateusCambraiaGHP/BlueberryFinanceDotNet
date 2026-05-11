namespace BlueBerryFinance.API.Application.Features.Store
{
    public class RegisterStoreRequest
    {
        public string Name { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
    }
}
