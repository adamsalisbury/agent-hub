using System.Text.Json.Serialization;

namespace AgentHub.Core.Entities;

public class Agent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastCheckedInAt { get; set; }
    public AgentStatus Status { get; set; }
    public bool IsSystemAgent { get; set; }
    public string? AvatarSvg { get; set; }
    public string? JobTitle { get; set; }

    [JsonIgnore]
    public ICollection<Message> SentMessages { get; set; } = [];

    [JsonIgnore]
    public ICollection<Message> ReceivedMessages { get; set; } = [];
}
