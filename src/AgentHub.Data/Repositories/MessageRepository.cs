using AgentHub.Core.Entities;
using AgentHub.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgentHub.Data.Repositories;

public class MessageRepository(AgentHubDbContext dbContext) : IMessageRepository
{
    public async Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Messages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Message>> GetInboxAsync(
        Guid agentId,
        bool includeRead = false,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Messages
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .Where(m => m.RecipientId == agentId || m.IsBroadcast);

        if (!includeRead)
        {
            query = query.Where(m => !m.IsRead);
        }

        return await query
            .OrderByDescending(m => m.SentAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Message>> GetOutboxAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Messages
            .Include(m => m.Recipient)
            .Include(m => m.Attachments)
            .Where(m => m.SenderId == agentId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Message> CreateAsync(Message message, CancellationToken cancellationToken = default)
    {
        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync(cancellationToken);
        return message;
    }

    public async Task<Message> UpdateAsync(Message message, CancellationToken cancellationToken = default)
    {
        dbContext.Messages.Update(message);
        await dbContext.SaveChangesAsync(cancellationToken);
        return message;
    }
}
