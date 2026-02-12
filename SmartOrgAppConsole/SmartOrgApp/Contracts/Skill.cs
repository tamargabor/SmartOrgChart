namespace SmartOrgApp.Contracts;

/// <summary>
/// Represents a skill vertex in the organization graph.
/// </summary>
public class Skill : VertexBase
{
    /// <summary>
    /// Name of the skill (e.g., "C# Development", "Azure Cloud").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Proficiency level (e.g., "Beginner", "Intermediate", "Advanced", "Expert").
    /// </summary>
    public string Level { get; set; } = string.Empty;

    public Skill()
    {
        Label = "skill";
    }

    /// <summary>
    /// Converts all skill properties to Gremlin property syntax.
    /// </summary>
    public override Dictionary<string, object> ToGremlinProperties()
    {
        var properties = base.ToGremlinProperties();
        properties["name"] = Name;
        properties["level"] = Level;
        return properties;
    }
}
