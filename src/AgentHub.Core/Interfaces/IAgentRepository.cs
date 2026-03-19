using AgentHub.Core.Entities;

namespace AgentHub.Core.Interfaces;

public interface IAgentRepository
{
    Task<IReadOnlyList<Agent>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Agent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Agent?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Agent> CreateAsync(Agent agent, CancellationToken cancellationToken = default);
    Task<Agent> UpdateAsync(Agent agent, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
