using AgentHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgentHub.Data;

public class AgentHubDbContext(DbContextOptions<AgentHubDbContext> options) : DbContext(options)
{
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgentHubDbContext).Assembly);
    }
}
