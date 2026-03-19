namespace AgentHub.Api.Models;

public record AgentDto(
    Guid Id,
    string Name,
    string Description,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastCheckedInAt,
    bool IsSystemAgent
);

public record CreateAgentRequest(
    string Name,
    string Description
);

public record UpdateAgentRequest(
    string Name,
    string Description
);
