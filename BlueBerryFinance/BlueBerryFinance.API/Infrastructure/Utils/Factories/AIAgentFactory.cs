using BlueBerryFinance.API.Infrastructure.Utils.Enums;
using BlueBerryFinance.API.Infrastructure.Utils.Factories.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
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
        {
            _options = options.Value;
        }

        public AIAgent Create(AIProvider provider = AIProvider.OpenAI, ChatOptions? chatOptions = null)
        {
            return provider switch
            {
                AIProvider.OpenAI => CreateOpenAIAgent(chatOptions),
                _ => throw new InvalidOperationException($"Unsupported AI provider: {provider}")
            };
        }

        private AIAgent CreateOpenAIAgent(ChatOptions? chatOptions)
        {
            var client = new OpenAIClient(new ApiKeyCredential(_options.ApiKey))
                .GetChatClient(OpenAIModels.Gpt41)
                .AsAIAgent(new ChatClientAgentOptions()
                {
                    Name = "HelpfulAssistant",
                    ChatOptions = chatOptions
                }); ;

            return client;
        }
    }
}
