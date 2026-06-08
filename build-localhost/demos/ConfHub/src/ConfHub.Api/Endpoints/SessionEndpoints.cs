using ConfHub.Api.Models;
using ConfHub.Api.Services;

namespace ConfHub.Api.Endpoints;

/// <summary>
/// Maps all /sessions routes using .NET 8 Minimal API endpoint groups.
/// </summary>
public static class SessionEndpoints
{
    public static IEndpointRouteBuilder MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/sessions")
            .WithTags("Sessions")
            .WithOpenApi();

        // GET /sessions
        group.MapGet("/", async (ICosmosDbService db, CancellationToken ct) =>
        {
            var sessions = await db.GetSessionsAsync(ct);
            return Results.Ok(sessions);
        })
        .WithName("GetSessions")
        .WithSummary("Get all conference sessions");

        // GET /sessions/{id}
        group.MapGet("/{id}", async (string id, ICosmosDbService db, CancellationToken ct) =>
        {
            var session = await db.GetSessionAsync(id, ct);
            return session is null ? Results.NotFound() : Results.Ok(session);
        })
        .WithName("GetSession")
        .WithSummary("Get a session by ID");

        // POST /sessions
        group.MapPost("/", async (SessionRequest request, ICosmosDbService db, CancellationToken ct) =>
        {
            var session = MapToSession(request);
            var created = await db.CreateSessionAsync(session, ct);
            return Results.Created($"/sessions/{created.Id}", created);
        })
        .WithName("CreateSession")
        .WithSummary("Create a new session");

        // PUT /sessions/{id}
        group.MapPut("/{id}", async (string id, SessionRequest request, ICosmosDbService db, CancellationToken ct) =>
        {
            var existing = await db.GetSessionAsync(id, ct);
            if (existing is null) return Results.NotFound();

            var updated = MapToSession(request);
            updated.Id = id;
            var result = await db.UpdateSessionAsync(updated, ct);
            return Results.Ok(result);
        })
        .WithName("UpdateSession")
        .WithSummary("Update an existing session");

        // DELETE /sessions/{id}
        group.MapDelete("/{id}", async (string id, ICosmosDbService db, CancellationToken ct) =>
        {
            var existing = await db.GetSessionAsync(id, ct);
            if (existing is null) return Results.NotFound();

            await db.DeleteSessionAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("DeleteSession")
        .WithSummary("Delete a session");

        // POST /sessions/recommend — AI-powered recommendations
        group.MapPost("/recommend", async (
            RecommendRequest request,
            ICosmosDbService db,
            IAgentService agent,
            CancellationToken ct) =>
        {
            var sessions = await db.GetSessionsAsync(ct);
            var recommendation = await agent.RecommendSessionsAsync(request.Topic, sessions, request.Level, ct);
            return Results.Ok(recommendation);
        })
        .WithName("RecommendSessions")
        .WithSummary("Get AI-powered session recommendations based on a topic");

        return app;
    }

    private static Session MapToSession(SessionRequest request) => new()
    {
        Title = request.Title,
        Speaker = request.Speaker,
        Track = request.Track,
        Level = request.Level,
        Abstract = request.Abstract,
        StartTime = request.StartTime,
        EndTime = request.EndTime,
        Room = request.Room,
    };
}
