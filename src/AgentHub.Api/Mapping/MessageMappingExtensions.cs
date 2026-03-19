using AgentHub.Api.Models;
using AgentHub.Core.Entities;

namespace AgentHub.Api.Mapping;

public static class MessageMappingExtensions
{
    public static MessageDto ToDto(this Message message)
    {
        return new MessageDto(
            Id: message.Id,
            SenderId: message.SenderId,
            SenderName: message.Sender?.Name ?? string.Empty,
            RecipientId: message.RecipientId,
            RecipientName: message.Recipient?.Name,
            Subject: message.Subject,
            Body: message.Body,
            IsBroadcast: message.IsBroadcast,
            IsRead: message.IsRead,
            SentAt: message.SentAt,
            ReadAt: message.ReadAt,
            Attachments: message.Attachments.Select(a => a.ToDto()).ToList(),
            InReplyToMessageId: message.InReplyToMessageId
        );
    }

    public static AttachmentDto ToDto(this Attachment attachment)
    {
        return new AttachmentDto(
            Id: attachment.Id,
            MessageId: attachment.MessageId,
            FileName: attachment.FileName,
            ContentType: attachment.ContentType,
            FileSizeBytes: attachment.FileSizeBytes,
            UploadedAt: attachment.UploadedAt
        );
    }
}
