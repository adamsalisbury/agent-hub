using AgentHub.Core.Entities;

namespace AgentHub.Api.ViewModels;

public class DashboardViewModel
{
    public IReadOnlyList<AgentSummaryViewModel> Agents { get; set; } = [];
    public int OnlineCount => Agents.Count(a => a.Status == AgentStatus.Online);
    public int OfflineCount => Agents.Count(a => a.Status == AgentStatus.Offline);
}

public class AgentSummaryViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AgentStatus Status { get; set; }
    public DateTimeOffset? LastCheckedInAt { get; set; }
    public bool IsSystemAgent { get; set; }
    public string? AvatarSvg { get; set; }
    public string? JobTitle { get; set; }
    public string? CurrentTask { get; set; }
}
