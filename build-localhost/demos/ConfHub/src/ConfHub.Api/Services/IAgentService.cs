using ConfHub.Api.Models;

namespace ConfHub.Api.Services;

/// <summary>Abstraction for AI-powered session recommendations via Azure AI Foundry.</summary>
public interface IAgentService
{
    /// <summary>
    /// Returns AI-powered session recommendations for a given topic and optional level.
    /// </summary>
    Task<RecommendResponse> RecommendSessionsAsync(
        string topic,
        IReadOnlyList<Session> allSessions,
        int? level = null,
        CancellationToken ct = default);
}
