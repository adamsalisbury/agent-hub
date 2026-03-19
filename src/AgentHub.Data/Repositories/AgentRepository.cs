using AgentHub.Core.Entities;
using AgentHub.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgentHub.Data.Repositories;

public class AgentRepository(AgentHubDbContext dbContext) : IAgentRepository
{
    public async Task<IReadOnlyList<Agent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Agents
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Agent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Agents.FindAsync([id], cancellationToken);
    }

    public async Task<Agent?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await dbContext.Agents
            .FirstOrDefaultAsync(a => a.Name == name, cancellationToken);
    }

    public async Task<Agent> CreateAsync(Agent agent, CancellationToken cancellationToken = default)
    {
        dbContext.Agents.Add(agent);
        await dbContext.SaveChangesAsync(cancellationToken);
        return agent;
    }

    public async Task<Agent> UpdateAsync(Agent agent, CancellationToken cancellationToken = default)
    {
        dbContext.Agents.Update(agent);
        await dbContext.SaveChangesAsync(cancellationToken);
        return agent;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var agent = await dbContext.Agents.FindAsync([id], cancellationToken);
        if (agent is not null)
        {
            dbContext.Agents.Remove(agent);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Agents.AnyAsync(a => a.Id == id, cancellationToken);
    }
}
