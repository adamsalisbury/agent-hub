using AgentHub.Core.Entities;
using AgentHub.Core.Interfaces;
using AgentHub.Data.JsonStore;

namespace AgentHub.Data.Repositories;

public class MessageRepository(
    JsonFileStore<Message> messageStore,
    JsonFileStore<Agent> agentStore,
    JsonFileStore<Attachment> attachmentStore) : IMessageRepository
{
    public async Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var messages = await messageStore.LoadAsync(cancellationToken);
        var message = messages.FirstOrDefault(m => m.Id == id);
        if (message is null) return null;

        await HydrateAsync(message, cancellationToken);
        return message;
    }

    public async Task<IReadOnlyList<Message>> GetInboxAsync(
        Guid agentId,
        bool includeRead = false,
        CancellationToken cancellationToken = default)
    {
        var messages = await messageStore.LoadAsync(cancellationToken);

        var inbox = messages
            .Where(m => m.RecipientId == agentId || m.IsBroadcast)
            .Where(m => includeRead || !m.IsRead)
            .OrderByDescending(m => m.SentAt)
            .ToList();

        foreach (var message in inbox)
            await HydrateAsync(message, cancellationToken);

        return inbox;
    }

    public async Task<IReadOnlyList<Message>> GetOutboxAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var messages = await messageStore.LoadAsync(cancellationToken);

        var outbox = messages
            .Where(m => m.SenderId == agentId)
            .OrderByDescending(m => m.SentAt)
            .ToList();

        foreach (var message in outbox)
            await HydrateAsync(message, cancellationToken);

        return outbox;
    }

    public async Task<Message> CreateAsync(Message message, CancellationToken cancellationToken = default)
    {
        var messages = await messageStore.LoadAsync(cancellationToken);
        messages.Add(message);
        await messageStore.SaveAsync(messages, cancellationToken);
        return message;
    }

    public async Task<Message> UpdateAsync(Message message, CancellationToken cancellationToken = default)
    {
        var messages = await messageStore.LoadAsync(cancellationToken);
        var index = messages.FindIndex(m => m.Id == message.Id);
        if (index < 0)
            throw new InvalidOperationException($"Message with ID {message.Id} not found.");

        messages[index] = message;
        await messageStore.SaveAsync(messages, cancellationToken);
        return message;
    }

    /// <summary>
    /// Hydrates navigation properties (Sender, Recipient, Attachments) from the other stores.
    /// </summary>
    private async Task HydrateAsync(Message message, CancellationToken cancellationToken)
    {
        var agents = await agentStore.LoadAsync(cancellationToken);
        var attachments = await attachmentStore.LoadAsync(cancellationToken);

        message.Sender = agents.FirstOrDefault(a => a.Id == message.SenderId)!;
        message.Recipient = message.RecipientId.HasValue
            ? agents.FirstOrDefault(a => a.Id == message.RecipientId.Value)
            : null;
        message.Attachments = attachments.Where(a => a.MessageId == message.Id).ToList();
    }
}
