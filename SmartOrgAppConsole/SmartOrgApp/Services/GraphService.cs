using System.Text;
using Gremlin.Net.Driver;
using Gremlin.Net.Structure.IO.GraphSON;
using SmartOrgApp.Contracts;

namespace SmartOrgApp.Services;

/// <summary>
/// Service for interacting with Azure Cosmos DB Gremlin API.
/// Encapsulates GremlinClient and provides type-safe methods for graph operations.
/// </summary>
public class GraphService : IDisposable
{
    private readonly GremlinClient _client;
    private bool _disposed = false;

    /// <summary>
    /// Initializes the GraphService with Azure Cosmos DB connection parameters.
    /// Configures GremlinClient with GraphSON v2 serialization for Cosmos DB compatibility.
    /// </summary>
    public GraphService(string hostname, string masterKey, string database, string container)
    {
        // Clean hostname (remove protocol prefixes)
        hostname = hostname.Replace("https://", "").Replace("wss://", "").Split(':')[0].Trim('/');

        // Configure Gremlin server connection
        var server = new GremlinServer(
            hostname,
            443,
            enableSsl: true,
            username: $"/dbs/{database}/colls/{container}",
            password: masterKey
        );

        // CRITICAL: Use GraphSON v2 to avoid MalformedRequest errors with Cosmos DB
        _client = new GremlinClient(
            server,
            new GraphSON2Reader(),
            new GraphSON2Writer(),
            GremlinClient.GraphSON2MimeType
        );
    }

    /// <summary>
    /// Tests the connection to the Gremlin server.
    /// </summary>
    /// <returns>The current vertex count in the graph.</returns>
    public async Task<long> TestConnectionAsync()
    {
        return await _client.SubmitWithSingleResultAsync<long>("g.V().count()");
    }

    /// <summary>
    /// Adds a vertex to the graph using the provided vertex entity.
    /// Constructs a Gremlin query programmatically from the vertex properties.
    /// </summary>
    public async Task AddVertexAsync<T>(T vertex) where T : VertexBase
    {
        var properties = vertex.ToGremlinProperties();
        
        // Build the Gremlin query using a query builder pattern
        var queryBuilder = new StringBuilder($"g.addV('{vertex.Label}')");
        
        foreach (var prop in properties)
        {
            // Escape single quotes in string values
            var value = prop.Value?.ToString()?.Replace("'", "\\'") ?? "";
            queryBuilder.Append($".property('{prop.Key}', '{value}')");
        }

        await _client.SubmitAsync(queryBuilder.ToString());
    }

    /// <summary>
    /// Adds an edge between two vertices.
    /// Constructs a Gremlin query programmatically from the edge entity.
    /// </summary>
    public async Task AddEdgeAsync(EdgeBase edge)
    {
        // Build query: g.V('fromId').addE('label').to(g.V('toId'))
        var query = $"g.V('{edge.FromId}').addE('{edge.Label}').to(g.V('{edge.ToId}'))";
        await _client.SubmitAsync(query);
    }

    /// <summary>
    /// Removes all vertices and edges from the graph.
    /// </summary>
    public async Task ClearGraphAsync()
    {
        await _client.SubmitAsync("g.V().drop()");
    }

    /// <summary>
    /// Gets the total count of vertices in the graph.
    /// </summary>
    public async Task<long> GetVertexCountAsync()
    {
        return await _client.SubmitWithSingleResultAsync<long>("g.V().count()");
    }

    /// <summary>
    /// Finds all skills associated with a specific person.
    /// Example traversal query demonstrating graph navigation.
    /// </summary>
    public async Task<List<string>> FindSkillsByPersonIdAsync(string personId)
    {
        // Query: Find all skills connected to the person via "hasSkill" edges
        var query = $"g.V('{personId}').out('hasSkill').values('name')";
        var results = await _client.SubmitAsync<string>(query);
        return results.ToList();
    }

    /// <summary>
    /// Finds all people who report to a specific person.
    /// </summary>
    public async Task<List<string>> FindDirectReportsAsync(string personId)
    {
        // Query: Find all people with incoming "reportsTo" edges to the specified person
        var query = $"g.V('{personId}').in('reportsTo').values('name')";
        var results = await _client.SubmitAsync<string>(query);
        return results.ToList();
    }

    /// <summary>
    /// Disposes the GremlinClient connection.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _client?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
