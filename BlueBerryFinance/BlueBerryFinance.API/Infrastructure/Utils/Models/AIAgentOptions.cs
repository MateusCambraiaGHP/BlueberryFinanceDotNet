using BlueBerryFinance.API.Infrastructure.Utils.Enums;

namespace BlueBerryFinance.API.Infrastructure.Utils.Models
{
    public class AIAgentOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public AIProvider Provider { get; set; } = AIProvider.OpenAI;
    }
}
