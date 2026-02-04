using BlueBerryFinance.API.Infrastructure.Utils.Enums;
using BlueBerryFinance.API.Infrastructure.Utils.Factories.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace BlueBerryFinance.API.Infrastructure.Utils.Factories
{
    public class AIAgentFactory : IAIAgentFactory
    {
        private readonly AIAgentOptions _options;


        public AIAgentFactory(IOptions<AIAgentOptions> options)
            => _options = options.Value;

        public AIAgent Create(
            string instructions, 
            string? model = null, 
            AIProvider provider = AIProvider.OpenAI)
        {
            var selectedModel = model ?? OpenAIModels.Gpt41;

            return provider switch
            {
                AIProvider.OpenAI => CreateOpenAIAgent(instructions, selectedModel),
                AIProvider.AzureOpenAI => throw new NotImplementedException("Azure OpenAI support coming soon"),
                AIProvider.Anthropic => throw new NotImplementedException("Anthropic support coming soon"),
                _ => throw new InvalidOperationException($"Unsupported AI provider: {provider}")
            };
        }

        private AIAgent CreateOpenAIAgent(string instructions, string model)
        {
            return new OpenAIClient(new ApiKeyCredential(_options.ApiKey))
                .GetChatClient(model)
                .AsAIAgent(instructions: instructions);
        }
    }
}
