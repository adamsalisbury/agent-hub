namespace AgentHub.Core.Entities;

public class AgentActivity
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public bool IsActive { get; set; }
}
