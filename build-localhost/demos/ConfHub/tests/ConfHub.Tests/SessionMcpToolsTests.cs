using ConfHub.Api.Mcp;
using ConfHub.Api.Models;
using ConfHub.Api.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ConfHub.Tests;

/// <summary>Unit tests for the MCP tool methods.</summary>
public class SessionMcpToolsTests
{
    private readonly ICosmosDbService _db = Substitute.For<ICosmosDbService>();
    private readonly IAgentService _agent = Substitute.For<IAgentService>();

    private static List<Session> BuildSessions() =>
    [
        new() { Id = "1", Title = "Intro to GitHub Copilot", Speaker = "Alice", Track = "AI", Level = 100, Abstract = "Learn the basics." },
        new() { Id = "2", Title = "Advanced Copilot Agent Mode", Speaker = "Bob", Track = "AI", Level = 300, Abstract = "Deep dive." },
        new() { Id = "3", Title = "Azure Cosmos DB at Scale", Speaker = "Carol", Track = "Azure", Level = 200, Abstract = "Global apps." },
        new() { Id = "4", Title = "DevOps with GitHub Actions", Speaker = "Dave", Track = "DevOps", Level = 200, Abstract = "CI/CD." },
    ];

    [Fact]
    public async Task GetSessionsAsync_ReturnsAllSessions_WhenNoFilter()
    {
        var sessions = BuildSessions();
        _db.GetSessionsAsync(Arg.Any<CancellationToken>()).Returns(sessions);
        var tools = new SessionMcpTools(_db, _agent);

        var result = await tools.GetSessionsAsync();

        result.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetSessionsAsync_FiltersByTrack()
    {
        var sessions = BuildSessions();
        _db.GetSessionsAsync(Arg.Any<CancellationToken>()).Returns(sessions);
        var tools = new SessionMcpTools(_db, _agent);

        var result = await tools.GetSessionsAsync(track: "AI");

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(s => s.Track.Should().Be("AI"));
    }

    [Fact]
    public async Task GetSessionsAsync_FiltersByLevel()
    {
        var sessions = BuildSessions();
        _db.GetSessionsAsync(Arg.Any<CancellationToken>()).Returns(sessions);
        var tools = new SessionMcpTools(_db, _agent);

        var result = await tools.GetSessionsAsync(level: 200);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(s => s.Level.Should().Be(200));
    }

    [Fact]
    public async Task GetSessionByIdAsync_ReturnsSession()
    {
        var session = BuildSessions()[0];
        _db.GetSessionAsync("1", Arg.Any<CancellationToken>()).Returns(session);
        var tools = new SessionMcpTools(_db, _agent);

        var result = await tools.GetSessionByIdAsync("1");

        result.Should().NotBeNull();
        result!.Id.Should().Be("1");
    }

    [Fact]
    public async Task GetTracksAsync_ReturnsDistinctTracks()
    {
        var sessions = BuildSessions();
        _db.GetSessionsAsync(Arg.Any<CancellationToken>()).Returns(sessions);
        var tools = new SessionMcpTools(_db, _agent);

        var tracks = await tools.GetTracksAsync();

        tracks.Should().BeEquivalentTo(["AI", "Azure", "DevOps"]);
    }

    [Fact]
    public async Task GetSpeakersAsync_ReturnsDistinctSpeakers()
    {
        var sessions = BuildSessions();
        _db.GetSessionsAsync(Arg.Any<CancellationToken>()).Returns(sessions);
        var tools = new SessionMcpTools(_db, _agent);

        var speakers = await tools.GetSpeakersAsync();

        speakers.Should().HaveCount(4);
        speakers.Should().Contain("Alice");
    }

    [Fact]
    public async Task RecommendSessionsAsync_CallsAgentWithSessions()
    {
        var sessions = BuildSessions();
        _db.GetSessionsAsync(Arg.Any<CancellationToken>()).Returns(sessions);
        var expected = new RecommendResponse("Try session 1", sessions);
        _agent.RecommendSessionsAsync("AI tools", sessions, null, Arg.Any<CancellationToken>()).Returns(expected);
        var tools = new SessionMcpTools(_db, _agent);

        var result = await tools.RecommendSessionsAsync("AI tools", level: 0);

        result.Recommendation.Should().Be("Try session 1");
    }
}
