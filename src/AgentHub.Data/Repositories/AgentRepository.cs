using AgentHub.Core.Entities;
using AgentHub.Core.Interfaces;
using AgentHub.Data.JsonStore;

namespace AgentHub.Data.Repositories;

public class AgentRepository(JsonFileStore<Agent> store) : IAgentRepository
{
    public async Task<IReadOnlyList<Agent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var agents = await store.LoadAsync(cancellationToken);
        return agents.OrderBy(a => a.Name).ToList();
    }

    public async Task<Agent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var agents = await store.LoadAsync(cancellationToken);
        return agents.FirstOrDefault(a => a.Id == id);
    }

    public async Task<Agent?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var agents = await store.LoadAsync(cancellationToken);
        return agents.FirstOrDefault(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Agent> CreateAsync(Agent agent, CancellationToken cancellationToken = default)
    {
        var agents = await store.LoadAsync(cancellationToken);
        agents.Add(agent);
        await store.SaveAsync(agents, cancellationToken);
        return agent;
    }

    public async Task<Agent> UpdateAsync(Agent agent, CancellationToken cancellationToken = default)
    {
        var agents = await store.LoadAsync(cancellationToken);
        var index = agents.FindIndex(a => a.Id == agent.Id);
        if (index < 0)
            throw new InvalidOperationException($"Agent with ID {agent.Id} not found.");

        agents[index] = agent;
        await store.SaveAsync(agents, cancellationToken);
        return agent;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var agents = await store.LoadAsync(cancellationToken);
        var removed = agents.RemoveAll(a => a.Id == id);
        if (removed > 0)
            await store.SaveAsync(agents, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var agents = await store.LoadAsync(cancellationToken);
        return agents.Any(a => a.Id == id);
    }
}
