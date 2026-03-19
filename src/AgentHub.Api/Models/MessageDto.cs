namespace AgentHub.Api.Models;

public record MessageDto(
    Guid Id,
    Guid SenderId,
    string SenderName,
    Guid? RecipientId,
    string? RecipientName,
    string Subject,
    string Body,
    bool IsBroadcast,
    bool IsRead,
    DateTimeOffset SentAt,
    DateTimeOffset? ReadAt,
    IReadOnlyList<AttachmentDto> Attachments,
    Guid? InReplyToMessageId
);

public record SendMessageRequest(
    Guid FromAgentId,
    string ToAgentId,  // agent Guid or "all" for broadcast
    string Subject,
    string Body,       // plain text or JSON
    Guid? InReplyToMessageId = null
);

public record AttachmentDto(
    Guid Id,
    Guid MessageId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    DateTimeOffset UploadedAt
);
