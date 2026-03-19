using AgentHub.Core.Entities;

namespace AgentHub.Core.Services;

/// <summary>
/// Computes agent online/offline status based on last check-in recency.
/// An agent is considered online if it checked in within the last 10 minutes.
/// </summary>
public static class AgentStatusService
{
    private static readonly TimeSpan OnlineThreshold = TimeSpan.FromMinutes(10);

    public static AgentStatus ComputeStatus(DateTimeOffset? lastCheckedInAt)
    {
        if (lastCheckedInAt is null)
            return AgentStatus.Offline;

        return DateTimeOffset.UtcNow - lastCheckedInAt.Value <= OnlineThreshold
            ? AgentStatus.Online
            : AgentStatus.Offline;
    }
}
