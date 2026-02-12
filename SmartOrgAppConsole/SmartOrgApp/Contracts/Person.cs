namespace SmartOrgApp.Contracts;

/// <summary>
/// Represents a person vertex in the organization graph.
/// </summary>
public class Person : VertexBase
{
    /// <summary>
    /// Full name of the person.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Job role or position (e.g., "Architect", "Developer", "Manager").
    /// </summary>
    public string Role { get; set; } = string.Empty;

    public Person()
    {
        Label = "person";
    }

    /// <summary>
    /// Converts all person properties to Gremlin property syntax.
    /// </summary>
    public override Dictionary<string, object> ToGremlinProperties()
    {
        var properties = base.ToGremlinProperties();
        properties["name"] = Name;
        properties["role"] = Role;
        return properties;
    }
}
