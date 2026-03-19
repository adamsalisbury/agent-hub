using AgentHub.Api.Mapping;
using AgentHub.Api.Models;
using AgentHub.Core.Entities;
using AgentHub.Core.Interfaces;

namespace AgentHub.Api.Endpoints;

public static class MessageEndpoints
{
    public static RouteGroupBuilder MapMessageEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/messages").WithTags("Messages");

        group.MapGet("/{id:guid}", async (Guid id, IMessageRepository repository, CancellationToken cancellationToken) =>
        {
            var message = await repository.GetByIdAsync(id, cancellationToken);
            return message is null ? Results.NotFound() : Results.Ok(message.ToDto());
        })
        .WithName("GetMessage")
        .WithSummary("Get a specific message by ID");

        group.MapGet("/inbox/{agentId:guid}", async (Guid agentId, IAgentRepository agentRepository, IMessageRepository messageRepository, CancellationToken cancellationToken) =>
        {
            if (!await agentRepository.ExistsAsync(agentId, cancellationToken))
                return Results.NotFound($"Agent {agentId} not found.");

            var messages = await messageRepository.GetInboxAsync(agentId, includeRead: false, cancellationToken);
            return Results.Ok(messages.Select(m => m.ToDto()));
        })
        .WithName("GetInbox")
        .WithSummary("Get agent's unread inbox messages");

        group.MapGet("/inbox/{agentId:guid}/all", async (Guid agentId, IAgentRepository agentRepository, IMessageRepository messageRepository, CancellationToken cancellationToken) =>
        {
            if (!await agentRepository.ExistsAsync(agentId, cancellationToken))
                return Results.NotFound($"Agent {agentId} not found.");

            var messages = await messageRepository.GetInboxAsync(agentId, includeRead: true, cancellationToken);
            return Results.Ok(messages.Select(m => m.ToDto()));
        })
        .WithName("GetInboxAll")
        .WithSummary("Get all inbox messages (read and unread)");

        group.MapGet("/outbox/{agentId:guid}", async (Guid agentId, IAgentRepository agentRepository, IMessageRepository messageRepository, CancellationToken cancellationToken) =>
        {
            if (!await agentRepository.ExistsAsync(agentId, cancellationToken))
                return Results.NotFound($"Agent {agentId} not found.");

            var messages = await messageRepository.GetOutboxAsync(agentId, cancellationToken);
            return Results.Ok(messages.Select(m => m.ToDto()));
        })
        .WithName("GetOutbox")
        .WithSummary("Get agent's sent messages");

        group.MapPost("/", async (SendMessageRequest request, IAgentRepository agentRepository, IMessageRepository messageRepository, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Subject))
                return Results.BadRequest("Message subject is required.");

            if (string.IsNullOrWhiteSpace(request.Body))
                return Results.BadRequest("Message body is required.");

            var sender = await agentRepository.GetByIdAsync(request.FromAgentId, cancellationToken);
            if (sender is null)
                return Results.BadRequest($"Sender agent {request.FromAgentId} not found.");

            bool isBroadcast = string.Equals(request.ToAgentId, "all", StringComparison.OrdinalIgnoreCase);
            Guid? recipientId = null;

            if (!isBroadcast)
            {
                if (!Guid.TryParse(request.ToAgentId, out var recipientGuid))
                    return Results.BadRequest("ToAgentId must be a valid GUID or 'all' for broadcast.");

                var recipient = await agentRepository.GetByIdAsync(recipientGuid, cancellationToken);
                if (recipient is null)
                    return Results.BadRequest($"Recipient agent {recipientGuid} not found.");

                recipientId = recipientGuid;
            }

            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = request.FromAgentId,
                RecipientId = recipientId,
                Subject = request.Subject,
                Body = request.Body,
                IsBroadcast = isBroadcast,
                IsRead = false,
                SentAt = DateTimeOffset.UtcNow,
                InReplyToMessageId = request.InReplyToMessageId
            };

            var created = await messageRepository.CreateAsync(message, cancellationToken);

            // Reload with navigation properties
            var fullMessage = await messageRepository.GetByIdAsync(created.Id, cancellationToken);
            return Results.Created($"/api/messages/{created.Id}", fullMessage!.ToDto());
        })
        .WithName("SendMessage")
        .WithSummary("Send a message to a specific agent or broadcast to all");

        group.MapPost("/{id:guid}/read", async (Guid id, IMessageRepository repository, CancellationToken cancellationToken) =>
        {
            var message = await repository.GetByIdAsync(id, cancellationToken);
            if (message is null) return Results.NotFound();

            if (!message.IsRead)
            {
                message.IsRead = true;
                message.ReadAt = DateTimeOffset.UtcNow;
                await repository.UpdateAsync(message, cancellationToken);
            }

            return Results.Ok(message.ToDto());
        })
        .WithName("MarkMessageRead")
        .WithSummary("Mark a message as read");

        return group;
    }
}
