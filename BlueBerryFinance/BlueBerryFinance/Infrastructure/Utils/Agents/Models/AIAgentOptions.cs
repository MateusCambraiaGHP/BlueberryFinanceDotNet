using BlueBerryFinance.API.Infrastructure.Utils.Agents.Enums;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Models
{
    public class AIAgentOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public AIProvider Provider { get; set; } = AIProvider.OpenAI;
    }
}
