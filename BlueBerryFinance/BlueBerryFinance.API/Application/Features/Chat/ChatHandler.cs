using BlueBerryFinance.API.Application.Features.Chat;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Tools.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Factories.Interfaces;
using BlueBerryFinance.API.Infrastructure.Utils.Helpers.Interfaces;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;

namespace BlueBerryFinance.API.Application.Features.Chat
{
    public class ChatHandler : IChatHandler
    {
        private readonly IAIAgentFactory _factory;
        private readonly IList<AITool> _tools;
        private readonly string _instructions;

        public ChatHandler(
            IAIAgentFactory factory,
            IOrchestratorTools orchestratorTools,
            IPromptLoader promptLoader)
        {
            _factory = factory;
            _tools = orchestratorTools.GetTools();
            _instructions = promptLoader.Load("Infrastructure.Utils.Agents.Finance.Prompts.FinantialAssistant.md");
        }

        public async IAsyncEnumerable<string> StreamAsync(
            string prompt,
            bool includeTools = true,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var client = _factory.CreateChatClient();
            var options = includeTools ? new ChatOptions { Tools = _tools } : new ChatOptions();

            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, _instructions),
                new(ChatRole.User, prompt)
            };

            while (!ct.IsCancellationRequested)
            {
                var updates = new List<ChatResponseUpdate>();

                await foreach (var update in client.GetStreamingResponseAsync(messages, options, ct))
                {
                    foreach (var content in update.Contents ?? [])
                    {
                        if (content is TextContent tc && !string.IsNullOrEmpty(tc.Text))
                            yield return tc.Text;
                    }

                    updates.Add(update);
                }

                var response = updates.ToChatResponse();

                if (response.FinishReason != ChatFinishReason.ToolCalls)
                    break;

                foreach (var msg in response.Messages)
                    messages.Add(msg);

                foreach (var msg in response.Messages)
                {
                    foreach (var call in msg.Contents.OfType<FunctionCallContent>())
                    {
                        var tool = _tools.OfType<AIFunction>()
                            .FirstOrDefault(f => f.Name == call.Name);

                        string toolResult;
                        if (tool is null)
                        {
                            toolResult = $"Tool '{call.Name}' not found.";
                        }
                        else
                        {
                            try
                            {
                                var args = call.Arguments is not null
                                    ? new AIFunctionArguments(call.Arguments)
                                    : new AIFunctionArguments();

                                var result = await tool.InvokeAsync(args, ct);
                                toolResult = result?.ToString() ?? string.Empty;
                            }
                            catch (Exception ex)
                            {
                                toolResult = $"Tool execution error: {ex.Message}";
                            }
                        }

                        messages.Add(new ChatMessage(ChatRole.Tool,
                            [new FunctionResultContent(call.CallId, toolResult)]));
                    }
                }
            }
        }
    }
}
