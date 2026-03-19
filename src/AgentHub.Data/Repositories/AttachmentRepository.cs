using AgentHub.Core.Entities;
using AgentHub.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgentHub.Data.Repositories;

public class AttachmentRepository(AgentHubDbContext dbContext) : IAttachmentRepository
{
    public async Task<Attachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Attachments.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<Attachment>> GetByMessageIdAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Attachments
            .Where(a => a.MessageId == messageId)
            .OrderBy(a => a.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Attachment> CreateAsync(Attachment attachment, CancellationToken cancellationToken = default)
    {
        dbContext.Attachments.Add(attachment);
        await dbContext.SaveChangesAsync(cancellationToken);
        return attachment;
    }
}
