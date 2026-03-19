using AgentHub.Core.Entities;

namespace AgentHub.Core.Interfaces;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Message>> GetInboxAsync(Guid agentId, bool includeRead = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Message>> GetOutboxAsync(Guid agentId, CancellationToken cancellationToken = default);
    Task<Message> CreateAsync(Message message, CancellationToken cancellationToken = default);
    Task<Message> UpdateAsync(Message message, CancellationToken cancellationToken = default);
}
