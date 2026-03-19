using AgentHub.Core.Entities;
using AgentHub.Core.Interfaces;

namespace AgentHub.Api.Services;

/// <summary>
/// Ensures the built-in System agent exists at startup.
/// </summary>
public class SystemAgentInitializer(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<SystemAgentInitializer> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var agentRepository = scope.ServiceProvider.GetRequiredService<IAgentRepository>();

        var systemAgentName = configuration["AgentHub:SystemAgentName"] ?? "System";
        var systemAgentDescription = configuration["AgentHub:SystemAgentDescription"]
            ?? "Built-in system operator agent for human oversight";

        var existingAgent = await agentRepository.GetByNameAsync(systemAgentName, cancellationToken);
        if (existingAgent is null)
        {
            var systemAgent = new Agent
            {
                Id = Guid.NewGuid(),
                Name = systemAgentName,
                Description = systemAgentDescription,
                CreatedAt = DateTimeOffset.UtcNow,
                Status = AgentStatus.Online,
                IsSystemAgent = true,
                LastCheckedInAt = DateTimeOffset.UtcNow
            };

            await agentRepository.CreateAsync(systemAgent, cancellationToken);
            logger.LogInformation("Created built-in System agent with ID {AgentId}", systemAgent.Id);
        }
        else
        {
            logger.LogInformation("System agent already exists with ID {AgentId}", existingAgent.Id);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
