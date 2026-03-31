namespace BlueBerryFinance.API.Infrastructure.Utils.Models
{
    public class AIAgentOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "http://local-dev-litellm:4000/v1";
        public string Model { get; set; } = "gpt-4.1-mini";
    }
}
