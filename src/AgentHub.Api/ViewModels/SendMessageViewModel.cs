namespace AgentHub.Api.ViewModels;

public class SendMessageViewModel
{
    public Guid FromAgentId { get; set; }
    public string ToAgentId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = "{}";
    public IReadOnlyList<AgentSummaryViewModel> Agents { get; set; } = [];
}
