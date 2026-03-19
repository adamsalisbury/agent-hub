using AgentHub.Core.Entities;
using AgentHub.Core.Interfaces;
using AgentHub.Data.JsonStore;

namespace AgentHub.Data.Repositories;

public class AgentActivityRepository(JsonFileStore<AgentActivity> store) : IAgentActivityRepository
{
    public async Task<IReadOnlyList<AgentActivity>> GetByAgentIdAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var activities = await store.LoadAsync(cancellationToken);
        return activities
            .Where(a => a.AgentId == agentId)
            .OrderByDescending(a => a.StartedAt)
            .ToList();
    }

    public async Task<AgentActivity?> GetActiveByAgentIdAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var activities = await store.LoadAsync(cancellationToken);
        return activities.FirstOrDefault(a => a.AgentId == agentId && a.IsActive);
    }

    public async Task<AgentActivity> CreateAsync(AgentActivity activity, CancellationToken cancellationToken = default)
    {
        var activities = await store.LoadAsync(cancellationToken);
        activities.Add(activity);
        await store.SaveAsync(activities, cancellationToken);
        return activity;
    }

    public async Task<AgentActivity> UpdateAsync(AgentActivity activity, CancellationToken cancellationToken = default)
    {
        var activities = await store.LoadAsync(cancellationToken);
        var index = activities.FindIndex(a => a.Id == activity.Id);
        if (index < 0)
            throw new InvalidOperationException($"AgentActivity with ID {activity.Id} not found.");

        activities[index] = activity;
        await store.SaveAsync(activities, cancellationToken);
        return activity;
    }
}
