namespace ConfHub.Api.Models;

/// <summary>Represents a conference session stored in Cosmos DB.</summary>
public class Session
{
    /// <summary>Unique identifier (Cosmos DB document id).</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Session title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Primary speaker name.</summary>
    public string Speaker { get; set; } = string.Empty;

    /// <summary>Conference track, e.g. "AI", "DevOps", "Azure Infrastructure".</summary>
    public string Track { get; set; } = string.Empty;

    /// <summary>Session level: 100 (intro), 200 (intermediate), 300 (advanced).</summary>
    public int Level { get; set; }

    /// <summary>Session abstract / description.</summary>
    public string Abstract { get; set; } = string.Empty;

    /// <summary>ISO 8601 start time.</summary>
    public DateTimeOffset? StartTime { get; set; }

    /// <summary>ISO 8601 end time.</summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>Room or location identifier.</summary>
    public string? Room { get; set; }
}

/// <summary>Request body for creating or updating a session.</summary>
public record SessionRequest(
    string Title,
    string Speaker,
    string Track,
    int Level,
    string Abstract,
    DateTimeOffset? StartTime = null,
    DateTimeOffset? EndTime = null,
    string? Room = null
);

/// <summary>Request body for AI-powered session recommendations.</summary>
public record RecommendRequest(string Topic, int? Level = null);

/// <summary>Response from the AI recommendation agent.</summary>
public record RecommendResponse(string Recommendation, IReadOnlyList<Session> Sessions);
