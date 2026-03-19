namespace AgentHub.Api.Models;

public record AgentDto(
    Guid Id,
    string Name,
    string Description,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastCheckedInAt,
    bool IsSystemAgent,
    string? AvatarSvg,
    string? JobTitle,
    string? CurrentTask
);

public record CreateAgentRequest(
    string Name,
    string Description,
    string? AvatarSvg = null,
    string? JobTitle = null
);

public record UpdateAgentRequest(
    string Name,
    string Description,
    string? AvatarSvg = null,
    string? JobTitle = null
);

public record UpdateTaskRequest(string Description);

public record AgentActivityDto(
    Guid Id,
    Guid AgentId,
    string Description,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    bool IsActive
);
