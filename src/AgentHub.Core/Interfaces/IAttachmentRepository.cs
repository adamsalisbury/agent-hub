using AgentHub.Core.Entities;

namespace AgentHub.Core.Interfaces;

public interface IAttachmentRepository
{
    Task<Attachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Attachment>> GetByMessageIdAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task<Attachment> CreateAsync(Attachment attachment, CancellationToken cancellationToken = default);
}
