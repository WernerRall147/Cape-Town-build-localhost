# GitHub Copilot Instructions — ConfHub

## Project Overview
ConfHub is a conference session tracker API built with:
- **.NET 8 Minimal API** for REST endpoints
- **Azure Cosmos DB** for session data persistence
- **Azure AI Foundry Agent Service** for AI-powered session recommendations
- **Model Context Protocol (MCP)** to expose data as Copilot-queryable tools
- **xUnit + NSubstitute + FluentAssertions** for testing

## Coding Conventions
- Use `ICosmosDbService` and `IAgentService` abstractions — never call Cosmos or AI Foundry directly in endpoints
- All endpoints live in `SessionEndpoints.cs` using `MapGroup`
- MCP tools live in `SessionMcpTools.cs` decorated with `[McpServerTool]`
- Use `async/await` and `CancellationToken` throughout
- Prefer `record` types for request/response DTOs
- Tests use NSubstitute for mocking — never use `Moq`
- Use FluentAssertions for assertions (`result.Should().Be(...)`)

## Azure Resource Naming (for this demo)
- Cosmos DB database: `confhub`
- Cosmos DB container: `sessions` (partition key: `/id`)
- AI Foundry project: Use your existing project or create `confhub-agent`

## Running Tests
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

## Useful Copilot Prompts for the Demo
- "Add a GET /sessions/speaker/{name} endpoint that returns all sessions by a given speaker"
- "Write unit tests for the new speaker endpoint with 100% coverage"
- "Refactor CosmosDbService to use DefaultAzureCredential instead of an auth key"
- "Add a PATCH /sessions/{id} endpoint to partially update a session"
