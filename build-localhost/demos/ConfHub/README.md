# Build //localhost Cape Town — Session Demo

## Ship It: Build, Test & Deploy AI-Powered Apps with GitHub Copilot and Azure AI Foundry

This folder contains everything needed to deliver the **Ship It** session — a fully
**repeatable**, end-to-end demo that goes from a blank VS Code window to a
production-ready, AI-powered cloud application.

It showcases the latest Microsoft developer stack:

- **GitHub Copilot agent mode** in VS Code
- **.NET 10** Minimal API
- **Azure Cosmos DB** (serverless, keyless via Microsoft Entra ID)
- **Azure AI Foundry** — GA Persistent Agents SDK
- **Model Context Protocol (MCP)** — grounding Copilot in live app data
- **Azure Developer CLI (azd) + Bicep** — one command up, one command down

---

## What You're Building

**ConfHub** — a conference session tracker with REST + AI recommendations + an MCP server.

```
┌─────────────────────────────────────────────────────────────────┐
│  VS Code + GitHub Copilot (agent mode)                          │
│                                                                 │
│  ┌─────────────────┐     ┌──────────────────┐                  │
│  │  ConfHub API    │────▶│  Azure Cosmos DB  │  (keyless / MI) │
│  │  (.NET 10)      │     │  (Sessions data)  │                  │
│  └────────┬────────┘     └──────────────────┘                  │
│           │                                                     │
│           │  Azure AI Foundry (Persistent Agents)              │
│           ▼                                                     │
│  ┌─────────────────┐     ┌──────────────────┐                  │
│  │  MCP Server     │◀────│  GitHub Copilot  │                  │
│  │  (built-in)     │     │  (MCP client)    │                  │
│  └─────────────────┘     └──────────────────┘                  │
│                                                                 │
│  Deployed with azd → Azure Container Apps                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## Prerequisites

| Tool | Install |
|------|---------|
| VS Code | https://code.visualstudio.com |
| GitHub Copilot extension | VS Code Extensions marketplace |
| .NET 10 SDK | https://dot.net |
| Azure CLI | https://aka.ms/az-cli |
| Azure Developer CLI (azd) | https://aka.ms/azd |
| Docker (for container deploy) | https://www.docker.com |
| Azure subscription | https://azure.microsoft.com/free |

---

## Quick Start (repeatable)

> All scripts read your Azure subscription/tenant from `.azure/.env` at the repo
> root (copy `.env.sample` → `.azure/.env`). They never sign you in — if your
> Azure context is wrong they print the exact `az login` command to run.

```powershell
# 0. Sign in to the target tenant + subscription (interactive)
az login --tenant <tenant_id>
az account set --subscription <subscription_id>

# 1. (Optional) create a service principal for CI/automation
./scripts/Setup-ServicePrincipal.ps1

# 2. Provision infrastructure + deploy the API (azd up)
./scripts/Provision.ps1

# 3. Seed sample conference sessions
./scripts/Seed-Cosmos.ps1

# 4. Tear everything down when you're done (fully repeatable)
./scripts/Cleanup.ps1
```

| Script | Purpose |
|--------|---------|
| `scripts/Setup-ServicePrincipal.ps1` | Creates `sp-confhub-demo` with the roles needed to provision + assign RBAC. Credentials saved to git-ignored `.azure/confhub.sp.json`. |
| `scripts/Provision.ps1` | `azd up` — provisions Cosmos DB, AI Foundry + model, Container Apps, identity & RBAC, then deploys the API. Writes local `appsettings.Development.json`. |
| `scripts/Seed-Cosmos.ps1` | POSTs `scripts/seed-data.json` to the API (`/sessions`). Works locally or against the deployed app. |
| `scripts/Cleanup.ps1` | `azd down --force --purge` + removes local secrets. `-DeleteServicePrincipal` also removes the SP. |

---

## Demo Flow (on stage)

### Step 1 — Setup (5 min)
Install the **GitHub Copilot** extension, sign in, open this folder in VS Code.

### Step 2 — Scaffold with Copilot agent mode (10 min)
Open Copilot Chat (`Ctrl+Shift+I`), switch to **Agent mode**, and prompt:
```
Create a .NET 10 Minimal API called ConfHub that manages conference sessions.
Each session has an id, title, speaker, track, level (100/200/300), and abstract.
Use Azure Cosmos DB (keyless, DefaultAzureCredential) with a CosmosDbService.
Add OpenAPI with the built-in .NET 10 document and a Scalar reference UI.
```

### Step 3 — Add an Azure AI Foundry agent (10 min)
```
Add an Azure AI Foundry AgentService using the GA Azure.AI.Agents.Persistent SDK
that recommends sessions for a topic, and expose POST /sessions/recommend.
```

### Step 4 — Tests & coverage (10 min)
```
Generate xUnit tests for the MCP tools and services with NSubstitute and
FluentAssertions. Run dotnet test with coverage.
```
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

### Step 5 — Push & open a PR (5 min)
Commit, push, open a PR — Copilot writes the summary and reviews inline.

### Step 6 — MCP server (10 min)
The MCP tools in `SessionMcpTools.cs` are already wired. With the API running,
`.vscode/mcp.json` connects Copilot to `http://localhost:5000/mcp`. Then ask:
> *"@confhub get all sessions at level 200"*

### Step 7 — Ship it (azd)
```powershell
./scripts/Provision.ps1   # deploys to Azure Container Apps
```

---

## Running Locally

```bash
cd build-localhost/demos/ConfHub

dotnet restore
dotnet build

# After Provision.ps1 has written appsettings.Development.json:
dotnet run --project src/ConfHub.Api
# → http://localhost:5000  (Scalar UI at /scalar, MCP at /mcp)

dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

---

## Configuration

`appsettings.json` (and the auto-generated `appsettings.Development.json`):

| Setting | Description |
|---------|-------------|
| `CosmosDb:AccountEndpoint` | Cosmos DB account URI |
| `CosmosDb:AuthKey` | **Leave empty** to use keyless Entra ID auth (recommended). Set only for quick key-based local runs. |
| `CosmosDb:DatabaseName` | `confhub` |
| `CosmosDb:ContainerName` | `sessions` |
| `AzureAIFoundry:ProjectEndpoint` | `https://<account>.services.ai.azure.com/api/projects/<project>` |
| `AzureAIFoundry:ModelDeploymentName` | e.g. `gpt-4o-mini` |
| `AzureAIFoundry:AgentId` | Optional — leave empty and the app creates the agent on first use |

---

## Repo Structure

```
build-localhost/demos/ConfHub/
├── azure.yaml                 ← azd service definition
├── nuget.config               ← repo-local NuGet source (reliable restore)
├── infra/                     ← Bicep IaC (Cosmos, Foundry, Container Apps, RBAC)
│   ├── main.bicep
│   ├── resources.bicep
│   └── main.parameters.json
├── scripts/                   ← repeatable provision / seed / cleanup (PowerShell)
├── src/ConfHub.Api/
│   ├── Models/Session.cs
│   ├── Services/CosmosDbService.cs
│   ├── Services/AgentService.cs   ← GA Persistent Agents SDK
│   ├── Endpoints/SessionEndpoints.cs
│   ├── Mcp/SessionMcpTools.cs
│   ├── Dockerfile
│   └── Program.cs
└── tests/ConfHub.Tests/
```

---

## Related Resources

- [GitHub Copilot Docs](https://docs.github.com/copilot)
- [Azure AI Foundry](https://ai.azure.com)
- [Azure AI Persistent Agents SDK (.NET)](https://learn.microsoft.com/dotnet/api/overview/azure/ai.agents.persistent-readme)
- [Model Context Protocol](https://modelcontextprotocol.io)
- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/)
- [.NET 10 Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
