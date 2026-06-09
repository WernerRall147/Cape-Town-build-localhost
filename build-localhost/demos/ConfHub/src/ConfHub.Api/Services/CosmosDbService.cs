using ConfHub.Api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace ConfHub.Api.Services;

/// <summary>
/// Implements <see cref="ICosmosDbService"/> using the Azure Cosmos DB .NET SDK.
/// </summary>
public sealed class CosmosDbService : ICosmosDbService
{
    private readonly Container _container;

    public CosmosDbService(CosmosClient client, IConfiguration configuration)
    {
        var db = configuration["CosmosDb:DatabaseName"] ?? "confhub";
        var container = configuration["CosmosDb:ContainerName"] ?? "sessions";
        _container = client.GetContainer(db, container);
    }

    public async Task<IReadOnlyList<Session>> GetSessionsAsync(CancellationToken ct = default)
    {
        var query = _container.GetItemLinqQueryable<Session>().ToFeedIterator();
        var sessions = new List<Session>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(ct);
            sessions.AddRange(response);
        }

        return sessions;
    }

    public async Task<Session?> GetSessionAsync(string id, CancellationToken ct = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<Session>(id, new PartitionKey(id), cancellationToken: ct);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Session> CreateSessionAsync(Session session, CancellationToken ct = default)
    {
        var response = await _container.CreateItemAsync(session, new PartitionKey(session.Id), cancellationToken: ct);
        return response.Resource;
    }

    public async Task<Session> UpdateSessionAsync(Session session, CancellationToken ct = default)
    {
        var response = await _container.ReplaceItemAsync(session, session.Id, new PartitionKey(session.Id), cancellationToken: ct);
        return response.Resource;
    }

    public async Task DeleteSessionAsync(string id, CancellationToken ct = default)
    {
        await _container.DeleteItemAsync<Session>(id, new PartitionKey(id), cancellationToken: ct);
    }
}
