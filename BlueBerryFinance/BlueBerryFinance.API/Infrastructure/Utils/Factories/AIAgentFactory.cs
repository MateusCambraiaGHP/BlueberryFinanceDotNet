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

        public AIAgent Create(ChatOptions? chatOptions = null)
        {
            return BuildOpenAIClient()
                .GetChatClient(_options.Model)
                .AsAIAgent(new ChatClientAgentOptions
                {
                    Name = "BlueberryFinancialAssistant",
                    ChatOptions = chatOptions
                });
        }

        public IChatClient CreateChatClient()
        {
            return BuildOpenAIClient()
                .GetChatClient(_options.Model)
                .AsIChatClient();
        }

        private OpenAIClient BuildOpenAIClient() =>
            new(new ApiKeyCredential(_options.ApiKey),
                new OpenAIClientOptions { Endpoint = new Uri(_options.BaseUrl) });
    }
}
