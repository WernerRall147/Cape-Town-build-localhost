using Azure.AI.Projects;
using Azure.Identity;
using ConfHub.Api.Models;

namespace ConfHub.Api.Services;

/// <summary>
/// Uses Azure AI Foundry Agent Service to recommend conference sessions
/// based on a topic and the current session catalogue.
/// </summary>
public sealed class AgentService : IAgentService
{
    private readonly AIProjectClient _projectClient;
    private readonly string _agentId;
    private readonly ILogger<AgentService> _logger;

    public AgentService(IConfiguration configuration, ILogger<AgentService> logger)
    {
        _logger = logger;
        var connectionString = configuration["AzureAIFoundry:ConnectionString"]
            ?? throw new InvalidOperationException("AzureAIFoundry:ConnectionString is not configured.");
        _agentId = configuration["AzureAIFoundry:AgentId"]
            ?? throw new InvalidOperationException("AzureAIFoundry:AgentId is not configured.");

        _projectClient = new AIProjectClient(connectionString, new DefaultAzureCredential());
    }

    public async Task<RecommendResponse> RecommendSessionsAsync(
        string topic,
        IReadOnlyList<Session> allSessions,
        int? level = null,
        CancellationToken ct = default)
    {
        var agentsClient = _projectClient.GetAgentsClient();

        // Build a context payload from the session catalogue
        var catalogue = BuildCatalogueText(allSessions, level);

        var thread = await agentsClient.CreateThreadAsync(cancellationToken: ct);

        var userMessage = $"""
            You are a helpful conference session recommender.

            Session catalogue:
            {catalogue}

            Based on the above sessions, recommend the best sessions for someone interested in: {topic}
            {(level.HasValue ? $"Preferred level: {level}" : string.Empty)}

            Return a friendly recommendation with session titles and brief reasons.
            """;

        await agentsClient.CreateMessageAsync(thread.Value.Id, MessageRole.User, userMessage, cancellationToken: ct);

        var run = await agentsClient.CreateRunAsync(thread.Value.Id, _agentId, cancellationToken: ct);

        // Poll until the run completes
        while (run.Value.Status == RunStatus.Queued || run.Value.Status == RunStatus.InProgress)
        {
            await Task.Delay(500, ct);
            run = await agentsClient.GetRunAsync(thread.Value.Id, run.Value.Id, cancellationToken: ct);
        }

        if (run.Value.Status != RunStatus.Completed)
        {
            _logger.LogWarning("Agent run finished with status {Status}", run.Value.Status);
            return new RecommendResponse("Unable to generate recommendations at this time.", allSessions);
        }

        var messages = await agentsClient.GetMessagesAsync(thread.Value.Id, cancellationToken: ct);
        var assistantReply = messages.Value.Data
            .Where(m => m.Role == MessageRole.Agent)
            .SelectMany(m => m.ContentItems.OfType<MessageTextContent>())
            .Select(c => c.Text)
            .FirstOrDefault() ?? "No recommendation generated.";

        // Filter sessions loosely matching the topic for the response payload
        var relevantSessions = allSessions
            .Where(s => level == null || s.Level == level)
            .ToList();

        return new RecommendResponse(assistantReply, relevantSessions);
    }

    private static string BuildCatalogueText(IReadOnlyList<Session> sessions, int? level)
    {
        var filtered = level.HasValue
            ? sessions.Where(s => s.Level == level).ToList()
            : sessions.ToList();

        return string.Join("\n", filtered.Select(s =>
            $"- [{s.Level}] \"{s.Title}\" by {s.Speaker} | Track: {s.Track} | {s.Abstract[..Math.Min(120, s.Abstract.Length)]}..."));
    }
}
