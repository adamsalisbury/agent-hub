using AgentHub.Core.Entities;
using AgentHub.Core.Interfaces;
using AgentHub.Data.JsonStore;
using AgentHub.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AgentHub.Data;

public static class ServiceExtensions
{
    public static IServiceCollection AddAgentHubData(
        this IServiceCollection services,
        Action<JsonStoreOptions>? configureOptions = null)
    {
        if (configureOptions is not null)
            services.Configure(configureOptions);
        else
            services.Configure<JsonStoreOptions>(_ => { });

        // Register the shared JSON file stores as singletons (thread-safe internally)
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<JsonStoreOptions>>().Value;
            return new JsonFileStore<Agent>(options.DataDirectory, "agents.json");
        });

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<JsonStoreOptions>>().Value;
            return new JsonFileStore<Message>(options.DataDirectory, "messages.json");
        });

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<JsonStoreOptions>>().Value;
            return new JsonFileStore<Attachment>(options.DataDirectory, "attachments.json");
        });

        // Register repositories
        services.AddScoped<IAgentRepository, AgentRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();

        return services;
    }
}
