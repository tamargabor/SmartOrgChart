namespace SmartOrgApp.Contracts;

/// <summary>
/// Abstract base class for all graph vertices.
/// Provides core properties required by Azure Cosmos DB Gremlin API.
/// </summary>
public abstract class VertexBase
{
    /// <summary>
    /// Unique identifier for the vertex.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The label/type of the vertex (e.g., "person", "skill").
    /// </summary>
    public string Label { get; protected set; } = string.Empty;

    /// <summary>
    /// Partition key required by Azure Cosmos DB for data distribution.
    /// </summary>
    public string PartitionKey { get; set; } = "org";

    /// <summary>
    /// Converts the vertex properties to Gremlin property syntax.
    /// Override this method in derived classes to include additional properties.
    /// </summary>
    /// <returns>Dictionary of property names and values for Gremlin queries.</returns>
    public virtual Dictionary<string, object> ToGremlinProperties()
    {
        return new Dictionary<string, object>
        {
            { "id", Id },
            { "partitionKey", PartitionKey }
        };
    }
}
