using System.Text.Json.Serialization;

namespace AgentHub.Core.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid? RecipientId { get; set; }  // null means broadcast to all
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;  // JSON body
    public bool IsBroadcast { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset SentAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public Guid? InReplyToMessageId { get; set; }

    [JsonIgnore]
    public Agent Sender { get; set; } = null!;

    [JsonIgnore]
    public Agent? Recipient { get; set; }

    [JsonIgnore]
    public ICollection<Attachment> Attachments { get; set; } = [];
}
