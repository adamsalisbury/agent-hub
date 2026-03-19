using AgentHub.Api.Mapping;
using AgentHub.Core.Entities;
using AgentHub.Core.Interfaces;

namespace AgentHub.Api.Endpoints;

public static class AttachmentEndpoints
{
    public static RouteGroupBuilder MapAttachmentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/messages").WithTags("Attachments");

        group.MapPost("/{id:guid}/attachments", async (
            Guid id,
            IFormFile file,
            IMessageRepository messageRepository,
            IAttachmentRepository attachmentRepository,
            IAttachmentStorageService storageService,
            CancellationToken cancellationToken) =>
        {
            var message = await messageRepository.GetByIdAsync(id, cancellationToken);
            if (message is null) return Results.NotFound($"Message {id} not found.");

            if (file.Length == 0)
                return Results.BadRequest("Uploaded file is empty.");

            await using var stream = file.OpenReadStream();
            var storagePath = await storageService.SaveAsync(id, file.FileName, stream, cancellationToken);

            var attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                MessageId = id,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                StoragePath = storagePath,
                UploadedAt = DateTimeOffset.UtcNow
            };

            var created = await attachmentRepository.CreateAsync(attachment, cancellationToken);
            return Results.Created($"/api/messages/{id}/attachments/{created.Id}", created.ToDto());
        })
        .WithName("UploadAttachment")
        .WithSummary("Upload a file attachment to a message")
        .DisableAntiforgery();

        group.MapGet("/{id:guid}/attachments/{attachmentId:guid}", async (
            Guid id,
            Guid attachmentId,
            IAttachmentRepository attachmentRepository,
            IAttachmentStorageService storageService,
            CancellationToken cancellationToken) =>
        {
            var attachment = await attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);
            if (attachment is null || attachment.MessageId != id)
                return Results.NotFound();

            var stream = await storageService.ReadAsync(attachment.StoragePath, cancellationToken);
            return Results.File(stream, attachment.ContentType, attachment.FileName);
        })
        .WithName("DownloadAttachment")
        .WithSummary("Download a file attachment");

        return group;
    }
}
