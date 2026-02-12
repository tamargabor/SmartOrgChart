namespace SmartOrgApp.Contracts;

/// <summary>
/// Represents an edge (relationship) between two vertices in the graph.
/// </summary>
public class EdgeBase
{
    /// <summary>
    /// ID of the source vertex.
    /// </summary>
    public string FromId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the target vertex.
    /// </summary>
    public string ToId { get; set; } = string.Empty;

    /// <summary>
    /// Label describing the relationship type (e.g., "reportsTo", "hasSkill", "knows").
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new edge with the specified parameters.
    /// </summary>
    public EdgeBase(string fromId, string toId, string label)
    {
        FromId = fromId;
        ToId = toId;
        Label = label;
    }

    /// <summary>
    /// Parameterless constructor for deserialization.
    /// </summary>
    public EdgeBase() { }
}
