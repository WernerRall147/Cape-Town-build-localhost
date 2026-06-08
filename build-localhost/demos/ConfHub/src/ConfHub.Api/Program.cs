using ConfHub.Api.Endpoints;
using ConfHub.Api.Mcp;
using ConfHub.Api.Services;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// ── Services ────────────────────────────────────────────────────────────────

// Azure Cosmos DB
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var endpoint = config["CosmosDb:AccountEndpoint"]
        ?? throw new InvalidOperationException("CosmosDb:AccountEndpoint is not configured.");
    var authKey = config["CosmosDb:AuthKey"]
        ?? throw new InvalidOperationException("CosmosDb:AuthKey is not configured.");
    return new CosmosClient(endpoint, authKey, new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    });
});
builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();

// Azure AI Foundry Agent
builder.Services.AddSingleton<IAgentService, AgentService>();

// MCP Server — exposes ConfHub data as MCP tools for GitHub Copilot
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<SessionMcpTools>();

// OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "ConfHub API",
        Version = "v1",
        Description = "Conference session management API with AI-powered recommendations and MCP server support."
    });
});

// ── App pipeline ────────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConfHub API v1"));
}

app.UseHttpsRedirection();

// REST endpoints
app.MapSessionEndpoints();

// MCP Server endpoint — GitHub Copilot connects here via .vscode/mcp.json
app.MapMcp("/mcp");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
