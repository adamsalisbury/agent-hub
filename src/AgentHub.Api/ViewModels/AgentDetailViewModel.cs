namespace AgentHub.Api.ViewModels;

public class AgentDetailViewModel
{
    public AgentSummaryViewModel Agent { get; set; } = new();
    public IReadOnlyList<MessageSummaryViewModel> Inbox { get; set; } = [];
    public IReadOnlyList<MessageSummaryViewModel> Outbox { get; set; } = [];
    public IReadOnlyList<AgentSummaryViewModel> AllAgents { get; set; } = [];
    public int UnreadCount => Inbox.Count(m => !m.IsRead);
}

public class MessageSummaryViewModel
{
    public Guid Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public bool IsBroadcast { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset SentAt { get; set; }
    public int AttachmentCount { get; set; }
}
