using Azure.AI.Agents.Persistent;
using Azure.Identity;
using ConfHub.Api.Models;

namespace ConfHub.Api.Services;

/// <summary>
/// Uses the Azure AI Foundry Agent Service (GA Persistent Agents SDK) to recommend
/// conference sessions based on a topic and the current session catalogue.
/// Authentication is keyless via Microsoft Entra ID (<see cref="DefaultAzureCredential"/>).
/// </summary>
public sealed class AgentService : IAgentService
{
    private readonly PersistentAgentsClient _client;
    private readonly string _modelDeploymentName;
    private readonly string? _configuredAgentId;
    private readonly ILogger<AgentService> _logger;
    private readonly SemaphoreSlim _agentLock = new(1, 1);
    private string? _agentId;

    private const string AgentName = "confhub-session-recommender";
    private const string AgentInstructions =
        "You are a friendly conference session recommender for the ConfHub event. " +
        "Given a catalogue of sessions and a topic of interest, recommend the most relevant " +
        "sessions. Reply with session titles and a short reason for each. Keep it concise.";

    public AgentService(IConfiguration configuration, ILogger<AgentService> logger)
    {
        _logger = logger;

        var projectEndpoint = configuration["AzureAIFoundry:ProjectEndpoint"]
            ?? throw new InvalidOperationException(
                "AzureAIFoundry:ProjectEndpoint is not configured. " +
                "Expected the form https://<resource>.services.ai.azure.com/api/projects/<project-name>.");

        _modelDeploymentName = configuration["AzureAIFoundry:ModelDeploymentName"] ?? "gpt-4o-mini";
        _configuredAgentId = configuration["AzureAIFoundry:AgentId"];

        _client = new PersistentAgentsClient(projectEndpoint, new DefaultAzureCredential());
    }

    public async Task<RecommendResponse> RecommendSessionsAsync(
        string topic,
        IReadOnlyList<Session> allSessions,
        int? level = null,
        CancellationToken ct = default)
    {
        var agentId = await EnsureAgentAsync(ct);
        var catalogue = BuildCatalogueText(allSessions, level);

        PersistentAgentThread thread = await _client.Threads.CreateThreadAsync(cancellationToken: ct);

        var userMessage = $"""
            Session catalogue:
            {catalogue}

            Recommend the best sessions for someone interested in: {topic}
            {(level.HasValue ? $"Preferred level: {level}" : string.Empty)}
            """;

        await _client.Messages.CreateMessageAsync(thread.Id, MessageRole.User, userMessage, cancellationToken: ct);

        ThreadRun run = await _client.Runs.CreateRunAsync(thread.Id, agentId, cancellationToken: ct);

        // Poll until the run reaches a terminal status.
        while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500), ct);
            run = await _client.Runs.GetRunAsync(thread.Id, run.Id, ct);
        }

        var relevantSessions = allSessions
            .Where(s => level == null || s.Level == level)
            .ToList();

        if (run.Status != RunStatus.Completed)
        {
            _logger.LogWarning("Agent run finished with status {Status}: {Error}", run.Status, run.LastError?.Message);
            return new RecommendResponse("Unable to generate recommendations at this time.", relevantSessions);
        }

        var assistantReply = await ReadAssistantReplyAsync(thread.Id, ct);
        return new RecommendResponse(assistantReply, relevantSessions);
    }

    private async Task<string> EnsureAgentAsync(CancellationToken ct)
    {
        if (_agentId is not null)
        {
            return _agentId;
        }

        await _agentLock.WaitAsync(ct);
        try
        {
            if (_agentId is not null)
            {
                return _agentId;
            }

            if (!string.IsNullOrWhiteSpace(_configuredAgentId))
            {
                _agentId = _configuredAgentId;
                return _agentId;
            }

            PersistentAgent agent = await _client.Administration.CreateAgentAsync(
                model: _modelDeploymentName,
                name: AgentName,
                instructions: AgentInstructions,
                cancellationToken: ct);

            _agentId = agent.Id;
            _logger.LogInformation("Created Foundry agent {AgentId} using model {Model}", _agentId, _modelDeploymentName);
            return _agentId;
        }
        finally
        {
            _agentLock.Release();
        }
    }

    private async Task<string> ReadAssistantReplyAsync(string threadId, CancellationToken ct)
    {
        await foreach (PersistentThreadMessage message in
            _client.Messages.GetMessagesAsync(threadId, order: ListSortOrder.Descending, cancellationToken: ct))
        {
            if (message.Role != MessageRole.Agent)
            {
                continue;
            }

            var text = message.ContentItems
                .OfType<MessageTextContent>()
                .Select(c => c.Text)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }

        return "No recommendation generated.";
    }

    private static string BuildCatalogueText(IReadOnlyList<Session> sessions, int? level)
    {
        var filtered = level.HasValue
            ? sessions.Where(s => s.Level == level).ToList()
            : sessions.ToList();

        return string.Join("\n", filtered.Select(s =>
            $"- [{s.Level}] \"{s.Title}\" by {s.Speaker} | Track: {s.Track} | {(s.Abstract.Length > 120 ? s.Abstract[..120] + "..." : s.Abstract)}"));
    }
}
