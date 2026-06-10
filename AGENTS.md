# AGENTS.md

## Project Overview

This repository contains the **Ship It** demo for **Build //localhost Cape Town**:
a fully repeatable, end-to-end build of an AI-powered cloud application using
GitHub Copilot agent mode, .NET 10, Azure Cosmos DB, Azure AI Foundry, MCP, and the
Azure Developer CLI (azd).

The demo app is **ConfHub** — a conference session tracker that exposes a REST API,
AI-powered recommendations (Azure AI Foundry Persistent Agents), and an MCP server
that GitHub Copilot can query for live data.

## Repository Structure

```
.
├── README.md                       # Session overview (aligned with the speaker deck)
└── build-localhost/
    ├── Ship It - Speaker Deck.pptx  # Presentation
    └── demos/ConfHub/              # The demo application
        ├── README.md               # Presenter walkthrough (exact steps)
        ├── azure.yaml              # azd service definition
        ├── nuget.config            # Repo-local NuGet source (reliable restore)
        ├── infra/                  # Bicep IaC (Cosmos, Foundry, Container Apps, RBAC)
        ├── scripts/                # PowerShell: provision / seed / cleanup / SP
        ├── src/ConfHub.Api/        # .NET 10 Minimal API
        └── tests/ConfHub.Tests/    # xUnit tests
```

## Tech Stack

- **.NET 10** Minimal API
- **Azure Cosmos DB** — serverless, keyless (Microsoft Entra ID / managed identity)
- **Azure AI Foundry** — `Azure.AI.Agents.Persistent` (GA) Persistent Agents SDK
- **Model Context Protocol** — `ModelContextProtocol.AspNetCore`
- **OpenAPI** — built-in .NET 10 document + Scalar UI
- **Testing** — xUnit, NSubstitute, FluentAssertions, Coverlet
- **IaC / deploy** — Bicep + Azure Developer CLI (azd) → Azure Container Apps

## Build & Test

All commands run from `build-localhost/demos/ConfHub`:

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

Run the API locally (after provisioning): `dotnet run --project src/ConfHub.Api`
→ http://localhost:5000 (Scalar at `/scalar`, MCP at `/mcp`).

> A repo-local `nuget.config` pins the `nuget.org` source so restore is reliable
> even if machine-level NuGet sources are misconfigured.

## Provision & Cleanup (repeatable)

Scripts read the subscription/tenant from `.azure/.env`. They never sign you in —
if the Azure context is wrong they print the exact `az login` command.

```powershell
./scripts/Setup-ServicePrincipal.ps1   # optional: SP for CI/automation
./scripts/Provision.ps1                 # azd up — infra + deploy
./scripts/Seed-Cosmos.ps1               # seed sample sessions via the API
./scripts/Cleanup.ps1                   # azd down --force --purge
```

Validate infrastructure changes with: `az bicep build --file infra/main.bicep`.

## Conventions

- Use `ICosmosDbService` / `IAgentService` abstractions — never call Cosmos or AI
  Foundry directly from endpoints.
- Endpoints live in `SessionEndpoints.cs` (`MapGroup`); MCP tools in
  `SessionMcpTools.cs` (`[McpServerTool]`).
- Prefer keyless auth (`DefaultAzureCredential` + managed identity); no secrets in
  source control.
- `async`/`await` with `CancellationToken` throughout; `record` types for DTOs.
- Tests use NSubstitute (never Moq) and FluentAssertions.

## Security

- `.azure/` (including `.env` and any `*.sp.json` credentials) is git-ignored.
- Cosmos DB and AI Foundry are provisioned with `disableLocalAuth: true`; access is
  via Microsoft Entra ID role assignments defined in `infra/resources.bicep`.

## Licensing

- Documentation/content: [Creative Commons Attribution 4.0](https://creativecommons.org/licenses/by/4.0/legalcode) (see `LICENSE`).
- Sample code: [MIT](https://opensource.org/licenses/MIT) (see `LICENSE-CODE`).
