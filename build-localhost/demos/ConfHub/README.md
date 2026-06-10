# ConfHub — Presenter Walkthrough

**Ship It! Build, test & deploy AI-powered apps with GitHub Copilot and Azure AI Foundry**
Build //localhost: Cape Town · session overview is in the [repo root README](../../../README.md).

This is the **exact, step-by-step script** for delivering the 45-minute live build.
ConfHub is a conference session tracker: a .NET 10 Minimal API on Azure Cosmos DB,
AI-powered recommendations via an Azure AI Foundry agent, and a built-in MCP server
GitHub Copilot can query — deployed to Azure Container Apps with `azd`.

---

## Before you go on stage

### Prerequisites

| Tool | Install |
|------|---------|
| VS Code | https://code.visualstudio.com |
| GitHub Copilot extension | VS Code Extensions marketplace (signed in) |
| .NET 10 SDK | https://dot.net |
| Azure CLI | https://aka.ms/az-cli |
| Azure Developer CLI (azd) | https://aka.ms/azd |
| Azure subscription | Cosmos DB + AI Foundry access, `gpt-4o-mini` quota |

> Docker is **not** required — the container image builds on Azure (ACR Tasks)
> via `remoteBuild: true` in `azure.yaml`.

### Dry-run checklist (do this the night before)

1. `cp .env.sample .azure/.env` and fill in `subscription_id` + `tenant_id`.
2. `az login --tenant <tenant_id>` then `az account set --subscription <subscription_id>`.
3. `dotnet test` (from this folder) → **14 tests pass**.
4. Run `./scripts/Provision.ps1` once end-to-end, confirm the deployed URL works,
   then `./scripts/Cleanup.ps1`. This warms image caches and surfaces any quota issues.
5. Pre-clone the GitHub repo and pre-open VS Code with Copilot signed in.

---

## The repeatable scripts

All scripts read your subscription/tenant from `.azure/.env`. They never sign you
in — if the Azure context is wrong they print the exact `az login` command.

| Script | What it does |
|--------|--------------|
| `scripts/Setup-ServicePrincipal.ps1` | Creates `sp-confhub-demo` (Contributor + RBAC Administrator). Credentials → git-ignored `.azure/confhub.sp.json`. Optional; only needed for unattended/CI runs. |
| `scripts/Provision.ps1` | `azd up` — provisions Cosmos DB, AI Foundry + `gpt-4o-mini`, Container Apps, identity & RBAC, then builds & deploys the API. Writes local `appsettings.Development.json`. |
| `scripts/Seed-Cosmos.ps1` | POSTs `scripts/seed-data.json` to the API (`/sessions`). Works against the deployed app or `http://localhost:5000`. |
| `scripts/Cleanup.ps1` | `azd down --force --purge` + removes local secrets. `-DeleteServicePrincipal` also deletes the SP. |

> **azd uses its own login.** If `azd up` says *"You must be logged into Azure"*,
> authenticate azd too: `azd auth login` (interactive), or non-interactively with
> the demo SP: `azd auth login --client-id <appId> --client-secret <pwd> --tenant-id <tenant>`.

---

## The live build — exact steps

### Segment 1 · Setup (0–5 min)

1. Open VS Code on an empty folder. Show the **GitHub Copilot** extension installed and signed in.
2. Open Copilot Chat (`Ctrl+Shift+I`) and switch the dropdown to **Agent** mode.
3. One line on why: *agent mode plans and executes multi-step tasks across files, runs commands, and fixes its own errors.*

### Segment 2 · Scaffold with agent mode (5–15 min)

4. Prompt Copilot (agent mode):
   ```
   Create a .NET 10 Minimal API called ConfHub that manages conference sessions.
   Each session has an id, title, speaker, track, level (100/200/300), and abstract.
   Use Azure Cosmos DB (keyless, DefaultAzureCredential) with a CosmosDbService
   behind an ICosmosDbService abstraction. Map CRUD endpoints with MapGroup.
   Add the built-in .NET 10 OpenAPI document and a Scalar reference UI.
   ```
5. Walk the generated files: `Models/Session.cs`, `Services/CosmosDbService.cs`,
   `Endpoints/SessionEndpoints.cs`, `Program.cs`.
6. `dotnet build` to show it compiles.

### Segment 3 · Test & coverage (15–25 min)

7. Prompt:
   ```
   Generate xUnit tests for the services and endpoints using NSubstitute for
   mocking and FluentAssertions. Cover filtering by track and level.
   ```
8. Run tests with coverage:
   ```bash
   dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
   ```
9. Call out: tests use **NSubstitute** (not Moq) and **FluentAssertions** — Copilot follows the repo conventions in `.github/copilot-instructions.md`.

### Segment 4 · Ship — push & PR (25–35 min)

10. `git checkout -b feature/confhub` → `git add .` → `git commit -m "feat: ConfHub API"`.
11. `git push` and open a Pull Request.
12. Show Copilot's **AI-generated PR summary**, then trigger an inline **Copilot review** and apply an agent-mode fix.

### Segment 5 · Extend — AI Agent + MCP (35–50 min)

13. Prompt:
    ```
    Add an Azure AI Foundry AgentService using the GA Azure.AI.Agents.Persistent SDK.
    Create the agent on first use, run a thread, and expose POST /sessions/recommend
    that takes { topic, level } and returns AI recommendations grounded in the catalogue.
    ```
14. Show `Mcp/SessionMcpTools.cs` — tools decorated with `[McpServerTool]` (get sessions, by id, recommend, tracks, speakers).
15. Deploy to Azure (the "ship it" moment):
    ```powershell
    ./scripts/Provision.ps1
    ./scripts/Seed-Cosmos.ps1
    ```
16. Hit the live endpoints (substitute your deployed URL):
    ```powershell
    $base = '<your Container App URL>'
    Invoke-RestMethod "$base/sessions"            # REST + Cosmos (keyless)
    Invoke-RestMethod "$base/sessions/recommend" -Method Post `
      -ContentType 'application/json' -Body (@{ topic = 'AI agents' } | ConvertTo-Json)
    ```
17. **MCP grounding finale.** With the API running, `.vscode/mcp.json` points Copilot
    at the MCP server. In Copilot Chat:
    > *"@confhub get all sessions at level 200"*
    > *"@confhub recommend sessions about developer productivity"*

    Copilot queries your **live app data** — the loop is closed.

### Segment 6 · Q&A (50–60 min)

18. Leave the deployed app and the Scalar UI (`/scalar`) on screen while you take questions.

---

## Running locally (optional, for rehearsal)

```bash
cd build-localhost/demos/ConfHub
dotnet restore
dotnet build

# After Provision.ps1 has written appsettings.Development.json:
dotnet run --project src/ConfHub.Api
# → http://localhost:5000  (Scalar UI at /scalar, MCP at /mcp)
```

---

## Configuration

`appsettings.json` (and the auto-generated `appsettings.Development.json`):

| Setting | Description |
|---------|-------------|
| `CosmosDb:AccountEndpoint` | Cosmos DB account URI |
| `CosmosDb:AuthKey` | **Leave empty** for keyless Entra ID auth (recommended). Set only for quick key-based local runs. |
| `CosmosDb:DatabaseName` | `confhub` |
| `CosmosDb:ContainerName` | `sessions` |
| `AzureAIFoundry:ProjectEndpoint` | `https://<account>.services.ai.azure.com/api/projects/<project>` |
| `AzureAIFoundry:ModelDeploymentName` | e.g. `gpt-4o-mini` |
| `AzureAIFoundry:AgentId` | Optional — leave empty and the app creates the agent on first use |

---

## Troubleshooting (things that bit us live)

| Symptom | Fix |
|---------|-----|
| `azd up` → *"You must be logged into Azure"* | azd has its own auth — run `azd auth login` (separate from `az login`). |
| Deploy fails: *container runtime not running* | Already handled — `azure.yaml` sets `remoteBuild: true` so the image builds on Azure, no local Docker. |
| Remote build fails: *Unable to find fallback package folder* | The `.dockerignore` excludes Windows `bin/`/`obj/` from the Linux build context. Keep it. |
| `/sessions/recommend` → 500, *lacks data action `agents/write`* | The app identity needs the **Foundry User** role (set in `infra/resources.bicep`). Allow ~2 min for the role to propagate after provisioning. |
| Wrong identity / `AuthorizationFailed` | Sign in as a **user** with Owner (or Contributor + User Access Administrator) on the target subscription; a bare service principal with no roles can't provision. |

---

## Repo structure

```
build-localhost/demos/ConfHub/
├── azure.yaml                 ← azd service definition (remoteBuild)
├── nuget.config               ← repo-local NuGet source (reliable restore)
├── infra/                     ← Bicep IaC (Cosmos, Foundry, Container Apps, RBAC)
│   ├── main.bicep
│   ├── resources.bicep
│   └── main.parameters.json
├── scripts/                   ← Setup-ServicePrincipal / Provision / Seed-Cosmos / Cleanup
├── src/ConfHub.Api/
│   ├── Models/Session.cs
│   ├── Services/CosmosDbService.cs
│   ├── Services/AgentService.cs   ← GA Persistent Agents SDK
│   ├── Endpoints/SessionEndpoints.cs
│   ├── Mcp/SessionMcpTools.cs
│   ├── Dockerfile · .dockerignore
│   └── Program.cs
└── tests/ConfHub.Tests/
```

---

## Related resources

- [GitHub Copilot Docs](https://docs.github.com/copilot)
- [Azure AI Foundry](https://ai.azure.com)
- [Azure AI Persistent Agents SDK (.NET)](https://learn.microsoft.com/dotnet/api/overview/azure/ai.agents.persistent-readme)
- [Model Context Protocol](https://modelcontextprotocol.io)
- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/)
- [.NET 10 Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
