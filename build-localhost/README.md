# Build //localhost Cape Town — Session Demo

## Ship It: Build, Test & Deploy AI-Powered Apps with GitHub Copilot and Azure AI Foundry

This folder contains everything needed to deliver the **Ship It** session at Build //localhost Cape Town.

---

## What You're Building

**ConfHub** — A conference session tracker that demonstrates a real end-to-end developer workflow using GitHub Copilot, .NET 8, Azure Cosmos DB, Azure AI Foundry, and MCP.

```
┌─────────────────────────────────────────────────────────────────┐
│  VS Code + GitHub Copilot (agent mode)                          │
│                                                                 │
│  ┌─────────────────┐     ┌──────────────────┐                  │
│  │  ConfHub API    │────▶│  Azure Cosmos DB  │                  │
│  │  (.NET 8)       │     │  (Sessions data)  │                  │
│  └────────┬────────┘     └──────────────────┘                  │
│           │                                                     │
│           │  Azure AI Foundry Agent                            │
│           ▼                                                     │
│  ┌─────────────────┐     ┌──────────────────┐                  │
│  │  MCP Server     │◀────│  GitHub Copilot  │                  │
│  │  (built-in)     │     │  (MCP client)    │                  │
│  └─────────────────┘     └──────────────────┘                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Prerequisites

| Tool | Install |
|------|---------|
| VS Code | https://code.visualstudio.com |
| GitHub Copilot extension | VS Code Extensions marketplace |
| .NET 8 SDK | https://dot.net |
| Azure CLI | https://aka.ms/az-cli |
| Azure subscription | https://azure.microsoft.com/free |

---

## Demo Flow

### Step 1 — Setup (5 min)
1. Open VS Code, install the **GitHub Copilot** extension
2. Sign in to GitHub Copilot
3. Clone this repo: `git clone https://github.com/WernerRall147/Cape-Town-build-localhost`

### Step 2 — Scaffold the App with Copilot Agent Mode (10 min)
1. Open GitHub Copilot Chat (`Ctrl+Shift+I`)
2. Switch to **Agent mode**
3. Prompt:
   ```
   Create a .NET 8 Minimal API called ConfHub that manages conference sessions.
   Each session has an id, title, speaker, track, level (100/200/300), and abstract.
   Use Azure Cosmos DB as the data store with a CosmosDbService.
   Add proper error handling and OpenAPI documentation.
   ```
4. Review Copilot's generated edits, run the app

### Step 3 — Add an Azure AI Foundry Agent (10 min)
1. Prompt Copilot:
   ```
   Add an Azure AI Foundry AgentService to ConfHub.Api that:
   - connects to my Azure AI Foundry project using AIProjectClient
   - exposes a POST /sessions/recommend endpoint
   - takes a { topic: string } body and returns AI-powered session recommendations
   ```
2. Configure `appsettings.json` with your Foundry endpoint

### Step 4 — Add Tests & Coverage (10 min)
1. Prompt Copilot:
   ```
   Generate xUnit tests for SessionService and AgentService with >80% coverage.
   Use NSubstitute for mocking. Add a coverlet.runsettings file.
   ```
2. Run: `dotnet test --collect:"XPlat Code Coverage"`

### Step 5 — Push & Open a PR (5 min)
1. `git add . && git commit -m "feat: initial ConfHub implementation"`
2. `git push origin feature/confhub`
3. Open a PR — Copilot auto-generates the PR summary
4. Show Copilot inline review suggestions

### Step 6 — MCP Server (10 min)
1. Show `SessionMcpTools.cs` — the MCP tools are already wired in
2. Add the MCP server to VS Code's `.vscode/mcp.json`:
   ```json
   {
     "servers": {
       "confhub": {
         "type": "http",
         "url": "http://localhost:5000/mcp"
       }
     }
   }
   ```
3. In Copilot Chat: `@confhub get all sessions at level 200`
4. Copilot queries your live app — groundedness in action!

---

## Running Locally

```bash
cd demos/ConfHub

# Restore and build
dotnet restore
dotnet build

# Run the API (update appsettings.Development.json first)
dotnet run --project src/ConfHub.Api

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

---

## Environment Variables

Copy `src/ConfHub.Api/appsettings.Development.json` and fill in:

| Setting | Description |
|---------|-------------|
| `CosmosDb:AccountEndpoint` | Your Cosmos DB account URI |
| `CosmosDb:AuthKey` | Cosmos DB primary key |
| `CosmosDb:DatabaseName` | e.g. `confhub` |
| `CosmosDb:ContainerName` | e.g. `sessions` |
| `AzureAIFoundry:ConnectionString` | Your AI Foundry project connection string |
| `AzureAIFoundry:AgentId` | Agent ID from AI Foundry portal |

---

## Repo Structure

```
build-localhost/
├── README.md                  ← You are here
├── session-details.md         ← Sessionize call-for-content
└── demos/
    └── ConfHub/
        ├── ConfHub.sln
        ├── src/
        │   └── ConfHub.Api/
        │       ├── Models/Session.cs
        │       ├── Services/CosmosDbService.cs
        │       ├── Services/AgentService.cs
        │       ├── Endpoints/SessionEndpoints.cs
        │       ├── Mcp/SessionMcpTools.cs
        │       └── Program.cs
        └── tests/
            └── ConfHub.Tests/
```

---

## GitHub Copilot MCP Configuration

The ConfHub API includes a built-in MCP server. To connect GitHub Copilot in VS Code, add `.vscode/mcp.json` to your workspace:

```json
{
  "servers": {
    "confhub": {
      "type": "http",
      "url": "http://localhost:5000/mcp"
    }
  }
}
```

Then in Copilot Chat you can ask things like:
- *"What sessions are available at level 200?"*
- *"Recommend sessions about AI for a beginner developer"*
- *"Who is speaking on the Azure track?"*

---

## Related Resources

- [GitHub Copilot Docs](https://docs.github.com/copilot)
- [Azure AI Foundry](https://ai.azure.com)
- [Model Context Protocol](https://modelcontextprotocol.io)
- [Azure Cosmos DB .NET SDK](https://aka.ms/cosmos-dotnet)
- [.NET 8 Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
