using System.Text.Json.Serialization;

namespace AgentHub.Core.Entities;

public class Attachment
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; set; }

    [JsonIgnore]
    public Message Message { get; set; } = null!;
}
