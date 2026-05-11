namespace BlueBerryFinance.API.Application.Features.Stores
{
    public class RegisterStoreRequest
    {
        public string Name { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
    }
}
