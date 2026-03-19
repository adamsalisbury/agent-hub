using AgentHub.Core.Entities;

namespace AgentHub.Core.Interfaces;

public interface IAgentActivityRepository
{
    Task<IReadOnlyList<AgentActivity>> GetByAgentIdAsync(Guid agentId, CancellationToken cancellationToken = default);
    Task<AgentActivity?> GetActiveByAgentIdAsync(Guid agentId, CancellationToken cancellationToken = default);
    Task<AgentActivity> CreateAsync(AgentActivity activity, CancellationToken cancellationToken = default);
    Task<AgentActivity> UpdateAsync(AgentActivity activity, CancellationToken cancellationToken = default);
}
