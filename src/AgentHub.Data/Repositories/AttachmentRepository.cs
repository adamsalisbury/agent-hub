using AgentHub.Core.Entities;
using AgentHub.Core.Interfaces;
using AgentHub.Data.JsonStore;

namespace AgentHub.Data.Repositories;

public class AttachmentRepository(JsonFileStore<Attachment> store) : IAttachmentRepository
{
    public async Task<Attachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var attachments = await store.LoadAsync(cancellationToken);
        return attachments.FirstOrDefault(a => a.Id == id);
    }

    public async Task<IReadOnlyList<Attachment>> GetByMessageIdAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var attachments = await store.LoadAsync(cancellationToken);
        return attachments
            .Where(a => a.MessageId == messageId)
            .OrderBy(a => a.UploadedAt)
            .ToList();
    }

    public async Task<Attachment> CreateAsync(Attachment attachment, CancellationToken cancellationToken = default)
    {
        var attachments = await store.LoadAsync(cancellationToken);
        attachments.Add(attachment);
        await store.SaveAsync(attachments, cancellationToken);
        return attachment;
    }
}
