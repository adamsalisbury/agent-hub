using AgentHub.Core.Interfaces;
using AgentHub.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AgentHub.Data;

public static class ServiceExtensions
{
    public static IServiceCollection AddAgentHubData(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AgentHubDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IAgentRepository, AgentRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();

        return services;
    }
}
