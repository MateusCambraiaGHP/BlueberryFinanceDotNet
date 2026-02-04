using BlueBerryFinance.API.Infrastructure.Utils.Agents.Enums;
using BlueBerryFinance.API.Infrastructure.Utils.Agents.Factories.Interfaces;
using Microsoft.Agents.AI;

namespace BlueBerryFinance.API.Infrastructure.Utils.Agents;

public abstract class AgentBase<T> : IAgentBase<T>
{
    protected readonly IAIAgentFactory _agentFactory;
    protected readonly string _instructions;

    protected AgentBase(
        IAIAgentFactory agentFactory,
        string instructions)
    {
        _agentFactory = agentFactory;
        _instructions = instructions;
    }

    public virtual async Task<T?> AskAsync(string prompt, Func<AgentResponse, T> parser, AIProvider provider = AIProvider.OpenAI)
    {
        var agent = _agentFactory.Create(_instructions, provider: provider);
        var response = await agent.RunAsync(prompt);
        return response is null ? default : parser(response);
    }
}