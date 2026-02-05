using BlueBerryFinance.API.Infrastructure.Utils.Enums;
using BlueBerryFinance.API.Infrastructure.Utils.Factories.Interfaces;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents;

public abstract class AgentBase<T> : IAgentBase<T> where T : class, new()
{
    protected readonly IAIAgentFactory _agentFactory;
    protected readonly string _instructions;
    protected readonly JsonElement _schema;
    protected readonly ChatOptions _chatOptions;

    protected AgentBase(
        IAIAgentFactory agentFactory,
        string instructions)
    {
        _agentFactory = agentFactory;
        _instructions = instructions;

        _schema = AIJsonUtilities.CreateJsonSchema(typeof(T));
        _chatOptions = new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                schema: _schema,
                schemaName: typeof(T).Name,
                schemaDescription: $"Structured response for {typeof(T).Name}"),
            Instructions = _instructions
        };
    }

    public virtual async Task<T?> AskAsync(string prompt, AIProvider provider = AIProvider.OpenAI)
    {
        var agent = _agentFactory.Create(provider: provider, chatOptions: _chatOptions);
        var response = await agent.RunAsync(prompt);

        if (response is null)
            return default;

        var content = response.ToString() ?? string.Empty;

        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}