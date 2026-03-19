using AgentHub.Api.Mapping;
using AgentHub.Api.Models;
using AgentHub.Core.Entities;
using AgentHub.Core.Interfaces;

namespace AgentHub.Api.Endpoints;

public static class AgentEndpoints
{
    public static RouteGroupBuilder MapAgentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/agents").WithTags("Agents");

        group.MapGet("/", async (IAgentRepository repository, CancellationToken cancellationToken) =>
        {
            var agents = await repository.GetAllAsync(cancellationToken);
            return Results.Ok(agents.Select(a => a.ToDto()));
        })
        .WithName("GetAllAgents")
        .WithSummary("List all agents");

        group.MapGet("/{id:guid}", async (Guid id, IAgentRepository repository, CancellationToken cancellationToken) =>
        {
            var agent = await repository.GetByIdAsync(id, cancellationToken);
            return agent is null ? Results.NotFound() : Results.Ok(agent.ToDto());
        })
        .WithName("GetAgent")
        .WithSummary("Get an agent by ID");

        group.MapPost("/", async (CreateAgentRequest request, IAgentRepository repository, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest("Agent name is required.");

            if (string.IsNullOrWhiteSpace(request.Description))
                return Results.BadRequest("Agent description is required.");

            var existing = await repository.GetByNameAsync(request.Name, cancellationToken);
            if (existing is not null)
                return Results.Conflict($"An agent named '{request.Name}' already exists.");

            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTimeOffset.UtcNow,
                Status = AgentStatus.Offline
            };

            var created = await repository.CreateAsync(agent, cancellationToken);
            return Results.Created($"/api/agents/{created.Id}", created.ToDto());
        })
        .WithName("CreateAgent")
        .WithSummary("Register a new agent");

        group.MapPut("/{id:guid}", async (Guid id, UpdateAgentRequest request, IAgentRepository repository, CancellationToken cancellationToken) =>
        {
            var agent = await repository.GetByIdAsync(id, cancellationToken);
            if (agent is null) return Results.NotFound();

            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest("Agent name is required.");

            if (string.IsNullOrWhiteSpace(request.Description))
                return Results.BadRequest("Agent description is required.");

            // Check name uniqueness if changing
            if (!string.Equals(agent.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await repository.GetByNameAsync(request.Name, cancellationToken);
                if (existing is not null)
                    return Results.Conflict($"An agent named '{request.Name}' already exists.");
            }

            agent.Name = request.Name;
            agent.Description = request.Description;

            var updated = await repository.UpdateAsync(agent, cancellationToken);
            return Results.Ok(updated.ToDto());
        })
        .WithName("UpdateAgent")
        .WithSummary("Update an agent");

        group.MapDelete("/{id:guid}", async (Guid id, IAgentRepository repository, CancellationToken cancellationToken) =>
        {
            var agent = await repository.GetByIdAsync(id, cancellationToken);
            if (agent is null) return Results.NotFound();

            if (agent.IsSystemAgent)
                return Results.BadRequest("The System agent cannot be deleted.");

            await repository.DeleteAsync(id, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteAgent")
        .WithSummary("Remove an agent");

        group.MapPost("/{id:guid}/checkin", async (Guid id, IAgentRepository repository, CancellationToken cancellationToken) =>
        {
            var agent = await repository.GetByIdAsync(id, cancellationToken);
            if (agent is null) return Results.NotFound();

            agent.LastCheckedInAt = DateTimeOffset.UtcNow;
            agent.Status = AgentStatus.Online;
            await repository.UpdateAsync(agent, cancellationToken);

            return Results.Ok(agent.ToDto());
        })
        .WithName("AgentCheckIn")
        .WithSummary("Agent heartbeat check-in");

        return group;
    }
}
