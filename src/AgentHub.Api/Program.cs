using AgentHub.Api.Endpoints;
using AgentHub.Api.Services;
using AgentHub.Core.Interfaces;
using AgentHub.Data;

var builder = WebApplication.CreateBuilder(args);

// JSON file-backed data store
var dataDirectory = builder.Configuration["AgentHub:DataDirectory"] ?? "data";
builder.Services.AddAgentHubData(options =>
{
    options.DataDirectory = dataDirectory;
});

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

// Always enable Swagger (useful in containers too)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Agent Hub API v1");
});

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
