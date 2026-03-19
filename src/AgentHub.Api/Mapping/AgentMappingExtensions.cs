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
            IsSystemAgent: agent.IsSystemAgent
        );
    }
}
