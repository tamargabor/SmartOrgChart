using SmartOrgApp.Contracts;

namespace SmartOrgApp.Services;

/// <summary>
/// Service for importing data from CSV files into the graph database.
/// </summary>
public class CsvImporter
{
    /// <summary>
    /// Imports people from a CSV file and adds them as vertices in the graph.
    /// Expected CSV format: Id,Name,Role,PartitionKey
    /// </summary>
    public async Task<int> ImportPeopleAsync(string filePath, GraphService graphService)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"People CSV file not found: {filePath}");
        }

        var lines = await File.ReadAllLinesAsync(filePath);
        int count = 0;

        // Skip header row
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(',');
            if (parts.Length < 4)
                continue;

            var person = new Person
            {
                Id = parts[0].Trim(),
                Name = parts[1].Trim(),
                Role = parts[2].Trim(),
                PartitionKey = parts[3].Trim()
            };

            await graphService.AddVertexAsync(person);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Imports skills from a CSV file and adds them as vertices in the graph.
    /// Expected CSV format: Id,Name,Level,PartitionKey
    /// </summary>
    public async Task<int> ImportSkillsAsync(string filePath, GraphService graphService)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Skills CSV file not found: {filePath}");
        }

        var lines = await File.ReadAllLinesAsync(filePath);
        int count = 0;

        // Skip header row
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(',');
            if (parts.Length < 4)
                continue;

            var skill = new Skill
            {
                Id = parts[0].Trim(),
                Name = parts[1].Trim(),
                Level = parts[2].Trim(),
                PartitionKey = parts[3].Trim()
            };

            await graphService.AddVertexAsync(skill);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Imports relationships from a CSV file and adds them as edges in the graph.
    /// Expected CSV format: FromId,ToId,Label
    /// </summary>
    public async Task<int> ImportRelationshipsAsync(string filePath, GraphService graphService)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Relationships CSV file not found: {filePath}");
        }

        var lines = await File.ReadAllLinesAsync(filePath);
        int count = 0;

        // Skip header row
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(',');
            if (parts.Length < 3)
                continue;

            var edge = new EdgeBase
            {
                FromId = parts[0].Trim(),
                ToId = parts[1].Trim(),
                Label = parts[2].Trim()
            };

            await graphService.AddEdgeAsync(edge);
            count++;
        }

        return count;
    }
}
