using ConfHub.Api.Models;
using ConfHub.Api.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace ConfHub.Api.Mcp;

/// <summary>
/// Exposes ConfHub session data as MCP tools so GitHub Copilot (or any MCP client)
/// can query live conference data using natural language.
/// </summary>
[McpServerToolType]
public sealed class SessionMcpTools
{
    private readonly ICosmosDbService _db;
    private readonly IAgentService _agent;

    public SessionMcpTools(ICosmosDbService db, IAgentService agent)
    {
        _db = db;
        _agent = agent;
    }

    [McpServerTool]
    [Description("Get all conference sessions. Optionally filter by track or level (100/200/300).")]
    public async Task<IReadOnlyList<Session>> GetSessionsAsync(
        [Description("Conference track to filter by, e.g. 'AI', 'DevOps'. Leave empty for all tracks.")] string? track = null,
        [Description("Session level to filter by: 100, 200, or 300. Leave 0 for all levels.")] int level = 0,
        CancellationToken ct = default)
    {
        var sessions = await _db.GetSessionsAsync(ct);

        return sessions
            .Where(s => string.IsNullOrEmpty(track) || s.Track.Equals(track, StringComparison.OrdinalIgnoreCase))
            .Where(s => level == 0 || s.Level == level)
            .ToList();
    }

    [McpServerTool]
    [Description("Get a specific conference session by its ID.")]
    public async Task<Session?> GetSessionByIdAsync(
        [Description("The unique ID of the session.")] string id,
        CancellationToken ct = default)
    {
        return await _db.GetSessionAsync(id, ct);
    }

    [McpServerTool]
    [Description("Get AI-powered session recommendations based on a topic of interest. Uses Azure AI Foundry to match sessions to your interests.")]
    public async Task<RecommendResponse> RecommendSessionsAsync(
        [Description("Topic or area of interest, e.g. 'machine learning', 'cloud security', 'developer productivity'.")] string topic,
        [Description("Preferred session level: 100 (intro), 200 (intermediate), 300 (advanced). Use 0 for any level.")] int level = 0,
        CancellationToken ct = default)
    {
        var sessions = await _db.GetSessionsAsync(ct);
        return await _agent.RecommendSessionsAsync(topic, sessions, level == 0 ? null : level, ct);
    }

    [McpServerTool]
    [Description("List all unique tracks available at the conference.")]
    public async Task<IReadOnlyList<string>> GetTracksAsync(CancellationToken ct = default)
    {
        var sessions = await _db.GetSessionsAsync(ct);
        return sessions.Select(s => s.Track).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(t => t).ToList();
    }

    [McpServerTool]
    [Description("List all speakers presenting at the conference.")]
    public async Task<IReadOnlyList<string>> GetSpeakersAsync(CancellationToken ct = default)
    {
        var sessions = await _db.GetSessionsAsync(ct);
        return sessions.Select(s => s.Speaker).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s).ToList();
    }
}
