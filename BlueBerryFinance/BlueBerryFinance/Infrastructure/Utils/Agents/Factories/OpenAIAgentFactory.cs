using BlueBerryFinance.API.Infrastructure.Utils.Agents.Factories.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents.Factories
{
    public class OpenAIAgentFactory(IOptions<OpenAIAgentOptions> options) : IAIAgentFactory
    {
        private readonly OpenAIAgentOptions _options = options.Value;

        public AIAgent Create(string instructions, string? model = null)
        {
            return new OpenAIClient(new ApiKeyCredential(_options.ApiKey))
                .GetChatClient(model ?? OpenAIModels.Gpt41)
                .AsAIAgent(instructions: instructions);
        }
    }
}
