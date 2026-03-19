using AgentHub.Api.Models;
using AgentHub.Core.Entities;
using AgentHub.Core.Services;

namespace AgentHub.Api.Mapping;

public static class AgentMappingExtensions
{
    public static AgentDto ToDto(this Agent agent)
    {
        var computedStatus = AgentStatusService.ComputeStatus(agent.LastCheckedInAt);
        return new AgentDto(
            Id: agent.Id,
            Name: agent.Name,
            Description: agent.Description,
            Status: computedStatus.ToString().ToLowerInvariant(),
            CreatedAt: agent.CreatedAt,
            LastCheckedInAt: agent.LastCheckedInAt,
            IsSystemAgent: agent.IsSystemAgent,
            AvatarSvg: agent.AvatarSvg,
            JobTitle: agent.JobTitle,
            CurrentTask: agent.CurrentTask,
            Skills: agent.Skills.Select(s => new AgentSkillDto(s.Name, s.Description)).ToList()
        );
    }

    public static AgentActivityDto ToDto(this AgentActivity activity)
    {
        return new AgentActivityDto(
            Id: activity.Id,
            AgentId: activity.AgentId,
            Description: activity.Description,
            StartedAt: activity.StartedAt,
            CompletedAt: activity.CompletedAt,
            IsActive: activity.IsActive
        );
    }
}
