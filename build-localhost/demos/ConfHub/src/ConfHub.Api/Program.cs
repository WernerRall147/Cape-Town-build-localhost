using Azure.Identity;
using ConfHub.Api.Endpoints;
using ConfHub.Api.Mcp;
using ConfHub.Api.Services;
using Microsoft.Azure.Cosmos;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Services ────────────────────────────────────────────────────────────────

// Azure Cosmos DB.
// Uses a keyless connection (Microsoft Entra ID via DefaultAzureCredential) when no
// AuthKey is supplied — the recommended production approach — and falls back to a
// key-based connection for quick local demos.
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var endpoint = config["CosmosDb:AccountEndpoint"]
        ?? throw new InvalidOperationException("CosmosDb:AccountEndpoint is not configured.");
    var authKey = config["CosmosDb:AuthKey"];

    var options = new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };

    return string.IsNullOrWhiteSpace(authKey)
        ? new CosmosClient(endpoint, new DefaultAzureCredential(), options)
        : new CosmosClient(endpoint, authKey, options);
});
builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();

// Azure AI Foundry Agent (keyless — Microsoft Entra ID)
builder.Services.AddSingleton<IAgentService, AgentService>();

// MCP Server — exposes ConfHub data as MCP tools for GitHub Copilot
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<SessionMcpTools>();

// Built-in OpenAPI document (.NET 10)
builder.Services.AddOpenApi();

// ── App pipeline ────────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // OpenAPI JSON at /openapi/v1.json + interactive Scalar reference at /scalar
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// REST endpoints
app.MapSessionEndpoints();

// MCP Server endpoint — GitHub Copilot connects here via .vscode/mcp.json
app.MapMcp("/mcp");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
