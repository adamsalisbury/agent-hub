using AgentHub.Api.Endpoints;
using AgentHub.Api.Services;
using AgentHub.Core.Interfaces;
using AgentHub.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=agent-hub.db";
builder.Services.AddAgentHubData(connectionString);

// Attachment storage
builder.Services.AddSingleton<IAttachmentStorageService, AttachmentStorageService>();

// System agent initializer
builder.Services.AddHostedService<SystemAgentInitializer>();

// MVC Controllers + Views
builder.Services.AddControllersWithViews();

// OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Agent Hub API",
        Version = "v1",
        Description = "Communication hub API for AI agents"
    });
});

var app = builder.Build();

// Run migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AgentHubDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Agent Hub API v1");
    });
}

app.UseStaticFiles();
app.UseRouting();

// MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// Minimal API routes
app.MapAgentEndpoints();
app.MapMessageEndpoints();
app.MapAttachmentEndpoints();

app.Run();
