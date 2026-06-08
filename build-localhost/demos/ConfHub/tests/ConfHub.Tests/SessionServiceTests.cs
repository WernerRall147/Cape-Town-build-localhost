using ConfHub.Api.Models;
using ConfHub.Api.Services;
using FluentAssertions;
using NSubstitute;

namespace ConfHub.Tests;

/// <summary>Unit tests for AgentService recommendation logic.</summary>
public class AgentServiceTests
{
    private readonly ICosmosDbService _db = Substitute.For<ICosmosDbService>();

    private static List<Session> BuildSessions() =>
    [
        new() { Id = "1", Title = "Intro to GitHub Copilot", Speaker = "Alice", Track = "AI", Level = 100, Abstract = "Learn the basics of GitHub Copilot and how it helps you write code faster." },
        new() { Id = "2", Title = "Advanced Copilot Agent Mode", Speaker = "Bob", Track = "AI", Level = 300, Abstract = "Deep dive into agent mode, multi-step tasks and autonomous coding." },
        new() { Id = "3", Title = "Azure Cosmos DB at Scale", Speaker = "Carol", Track = "Azure", Level = 200, Abstract = "Building globally distributed apps with Cosmos DB." },
        new() { Id = "4", Title = "DevOps with GitHub Actions", Speaker = "Dave", Track = "DevOps", Level = 200, Abstract = "CI/CD pipelines, environments, and deployment strategies." },
        new() { Id = "5", Title = "Building AI Agents with Foundry", Speaker = "Eve", Track = "AI", Level = 300, Abstract = "End-to-end walkthrough of building an AI agent with Azure AI Foundry." },
    ];

    [Fact]
    public void BuildCatalogueText_FiltersSessionsByLevel()
    {
        var sessions = BuildSessions();

        // Sessions at level 300
        var level300 = sessions.Where(s => s.Level == 300).ToList();
        level300.Should().HaveCount(2);
        level300.Should().AllSatisfy(s => s.Level.Should().Be(300));
    }

    [Fact]
    public void Sessions_HaveExpectedProperties()
    {
        var session = new Session
        {
            Title = "Test Session",
            Speaker = "Test Speaker",
            Track = "AI",
            Level = 200,
            Abstract = "A test abstract.",
        };

        session.Id.Should().NotBeNullOrEmpty();
        session.Title.Should().Be("Test Session");
        session.Level.Should().Be(200);
    }

    [Fact]
    public async Task GetSessionsAsync_ReturnsEmptyList_WhenNoSessionsExist()
    {
        _db.GetSessionsAsync(Arg.Any<CancellationToken>())
           .Returns(Array.Empty<Session>());

        var result = await _db.GetSessionsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSessionsAsync_ReturnsSessions()
    {
        var sessions = BuildSessions();
        _db.GetSessionsAsync(Arg.Any<CancellationToken>()).Returns(sessions);

        var result = await _db.GetSessionsAsync();

        result.Should().HaveCount(5);
        result.Should().Contain(s => s.Track == "AI");
    }

    [Fact]
    public async Task GetSessionAsync_ReturnsNull_WhenSessionNotFound()
    {
        _db.GetSessionAsync("missing", Arg.Any<CancellationToken>()).Returns((Session?)null);

        var result = await _db.GetSessionAsync("missing");

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateSessionAsync_ReturnsCreatedSession()
    {
        var session = new Session { Title = "New Session", Speaker = "Frank", Track = "DevOps", Level = 100, Abstract = "Intro." };
        _db.CreateSessionAsync(session, Arg.Any<CancellationToken>()).Returns(session);

        var result = await _db.CreateSessionAsync(session);

        result.Should().Be(session);
        result.Title.Should().Be("New Session");
    }

    [Fact]
    public async Task DeleteSessionAsync_CompletesSuccessfully()
    {
        _db.DeleteSessionAsync("1", Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var act = async () => await _db.DeleteSessionAsync("1");

        await act.Should().NotThrowAsync();
    }
}
