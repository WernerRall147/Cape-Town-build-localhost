using ConfHub.Api.Models;

namespace ConfHub.Api.Services;

/// <summary>Abstraction for Cosmos DB session data operations.</summary>
public interface ICosmosDbService
{
    Task<IReadOnlyList<Session>> GetSessionsAsync(CancellationToken ct = default);
    Task<Session?> GetSessionAsync(string id, CancellationToken ct = default);
    Task<Session> CreateSessionAsync(Session session, CancellationToken ct = default);
    Task<Session> UpdateSessionAsync(Session session, CancellationToken ct = default);
    Task DeleteSessionAsync(string id, CancellationToken ct = default);
}
