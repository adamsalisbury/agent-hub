namespace AgentHub.Api.ViewModels;

public class AgentDetailViewModel
{
    public AgentSummaryViewModel Agent { get; set; } = new();
    public IReadOnlyList<MessageSummaryViewModel> Inbox { get; set; } = [];
    public IReadOnlyList<MessageSummaryViewModel> Outbox { get; set; } = [];
    public IReadOnlyList<AgentSummaryViewModel> AllAgents { get; set; } = [];
    public IReadOnlyList<AgentActivityViewModel> Activities { get; set; } = [];
    public int UnreadCount => Inbox.Count(m => !m.IsRead);

    public IReadOnlyList<MessageSummaryViewModel> Conversation =>
        Inbox.Select(m => m with { IsOutgoing = false })
            .Concat(Outbox.Select(m => m with { IsOutgoing = true }))
            .OrderBy(m => m.SentAt)
            .ToList();
}

public class AgentActivityViewModel
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public bool IsActive { get; set; }

    public string FormattedDuration
    {
        get
        {
            var end = CompletedAt ?? DateTimeOffset.UtcNow;
            var duration = end - StartedAt;

            if (duration.TotalSeconds < 60)
                return $"{(int)duration.TotalSeconds}s";
            if (duration.TotalMinutes < 60)
                return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
            if (duration.TotalHours < 24)
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";

            return $"{(int)duration.TotalDays}d {duration.Hours}h";
        }
    }
}

public record MessageSummaryViewModel
{
    public Guid Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public bool IsBroadcast { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset SentAt { get; set; }
    public int AttachmentCount { get; set; }
    public bool IsOutgoing { get; set; }
    public Guid? InReplyToMessageId { get; set; }
    public string? InReplyToSubject { get; set; }
}
