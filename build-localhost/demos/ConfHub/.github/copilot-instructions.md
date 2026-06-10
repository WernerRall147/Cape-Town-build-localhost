# GitHub Copilot Instructions — ConfHub

## Project Overview
ConfHub is a conference session tracker API built with:
- **.NET 10 Minimal API** for REST endpoints
- **Azure Cosmos DB** (serverless, keyless via Microsoft Entra ID) for session data
- **Azure AI Foundry — GA Persistent Agents SDK** (`Azure.AI.Agents.Persistent`) for AI-powered recommendations
- **Model Context Protocol (MCP)** to expose data as Copilot-queryable tools
- **Built-in OpenAPI (.NET 10) + Scalar** for the interactive API reference
- **xUnit + NSubstitute + FluentAssertions** for testing
- **azd + Bicep** for repeatable provisioning to Azure Container Apps

## Coding Conventions
- Use `ICosmosDbService` and `IAgentService` abstractions — never call Cosmos or AI Foundry directly in endpoints
- All endpoints live in `SessionEndpoints.cs` using `MapGroup`
- MCP tools live in `SessionMcpTools.cs` decorated with `[McpServerTool]`
- Prefer keyless auth: `DefaultAzureCredential` + managed identity, no secrets in config
- Use `async/await` and `CancellationToken` throughout
- Prefer `record` types for request/response DTOs
- Tests use NSubstitute for mocking — never use `Moq`
- Use FluentAssertions for assertions (`result.Should().Be(...)`)

## Azure Resource Naming (for this demo)
- Cosmos DB database: `confhub`
- Cosmos DB container: `sessions` (partition key: `/id`)
- AI Foundry project: `confhub`; default model deployment: `gpt-4o-mini`
- Infrastructure is defined in `infra/*.bicep` and deployed with `azd up`

## Running Tests
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

## Provisioning & Cleanup (repeatable)
```powershell
./scripts/Provision.ps1     # azd up — infra + deploy
./scripts/Seed-Cosmos.ps1   # seed sample sessions via the API
./scripts/Cleanup.ps1       # azd down --force --purge
```

## Useful Copilot Prompts for the Demo
- "Add a GET /sessions/speaker/{name} endpoint that returns all sessions by a given speaker"
- "Write unit tests for the new speaker endpoint with 100% coverage"
- "Add a PATCH /sessions/{id} endpoint to partially update a session"
- "Add a new MCP tool that returns the count of sessions per track"
