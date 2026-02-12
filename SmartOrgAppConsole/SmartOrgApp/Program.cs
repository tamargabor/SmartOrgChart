using SmartOrgApp.Services;
using Gremlin.Net.Driver.Exceptions;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main(string[] args)
    {
        // ---------------------------------------------------------
        // 1. CONFIGURATION MANAGEMENT
        // ---------------------------------------------------------
        // Build configuration from appsettings.json files
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();

        // Read configuration values
        string hostname = configuration["CosmosDb:Hostname"] ?? throw new InvalidOperationException("CosmosDb:Hostname not found in configuration");
        string masterKey = configuration["CosmosDb:MasterKey"] ?? throw new InvalidOperationException("CosmosDb:MasterKey not found in configuration");
        string database = configuration["CosmosDb:Database"] ?? throw new InvalidOperationException("CosmosDb:Database not found in configuration");
        string container = configuration["CosmosDb:Container"] ?? throw new InvalidOperationException("CosmosDb:Container not found in configuration");

        Console.WriteLine("=================================================");
        Console.WriteLine("    Smart Organization Chart - Layered Edition");
        Console.WriteLine("=================================================");
        Console.WriteLine();

        try
        {
            // ---------------------------------------------------------
            // 2. INITIALIZE SERVICES
            // ---------------------------------------------------------
            Console.WriteLine("Initializing Graph Service...");
            using var graphService = new GraphService(hostname, masterKey, database, container);
            var csvImporter = new CsvImporter();

            // ---------------------------------------------------------
            // 3. TEST CONNECTION
            // ---------------------------------------------------------
            Console.Write("Testing connection to Cosmos DB... ");
            var initialCount = await graphService.TestConnectionAsync();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"SUCCESS! (Current vertices: {initialCount})");
            Console.ResetColor();
            Console.WriteLine();

            // ---------------------------------------------------------
            // 4. CLEAR EXISTING DATA
            // ---------------------------------------------------------
            Console.Write("Clearing existing graph data... ");
            await graphService.ClearGraphAsync();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("DONE");
            Console.ResetColor();
            Console.WriteLine();

            // ---------------------------------------------------------
            // 5. IMPORT DATA FROM CSV FILES
            // ---------------------------------------------------------
            Console.WriteLine("Importing data from CSV files:");
            Console.WriteLine("-------------------------------");

            // Import people
            Console.Write("  Importing people... ");
            var peopleCount = await csvImporter.ImportPeopleAsync("Data/people.csv", graphService);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{peopleCount} people imported");
            Console.ResetColor();

            // Import skills
            Console.Write("  Importing skills... ");
            var skillsCount = await csvImporter.ImportSkillsAsync("Data/skills.csv", graphService);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{skillsCount} skills imported");
            Console.ResetColor();

            // Import relationships
            Console.Write("  Importing relationships... ");
            var relationshipsCount = await csvImporter.ImportRelationshipsAsync("Data/relationships.csv", graphService);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{relationshipsCount} relationships imported");
            Console.ResetColor();
            Console.WriteLine();

            // ---------------------------------------------------------
            // 6. VERIFY IMPORT
            // ---------------------------------------------------------
            var finalCount = await graphService.GetVertexCountAsync();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Total vertices in graph: {finalCount}");
            Console.ResetColor();
            Console.WriteLine();

            // ---------------------------------------------------------
            // 7. SAMPLE QUERIES
            // ---------------------------------------------------------
            Console.WriteLine("=================================================");
            Console.WriteLine("    Running Sample Traversal Queries");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            // Query 1: Find skills of a specific person
            Console.WriteLine("QUERY 1: What skills does János have?");
            Console.WriteLine("---------------------------------------");
            var janosSkills = await graphService.FindSkillsByPersonIdAsync("Janos");
            if (janosSkills.Any())
            {
                foreach (var skill in janosSkills)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✓ {skill}");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("  No skills found.");
            }
            Console.WriteLine();

            // Query 2: Find direct reports
            Console.WriteLine("QUERY 2: Who reports directly to Béla?");
            Console.WriteLine("---------------------------------------");
            var belaReports = await graphService.FindDirectReportsAsync("Bela");
            if (belaReports.Any())
            {
                foreach (var person in belaReports)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✓ {person}");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("  No direct reports found.");
            }
            Console.WriteLine();

            // Query 3: Find skills of another person
            Console.WriteLine("QUERY 3: What skills does Péter have?");
            Console.WriteLine("---------------------------------------");
            var peterSkills = await graphService.FindSkillsByPersonIdAsync("Peter");
            if (peterSkills.Any())
            {
                foreach (var skill in peterSkills)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✓ {skill}");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("  No skills found.");
            }
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=================================================");
            Console.WriteLine("    All operations completed successfully!");
            Console.WriteLine("=================================================");
            Console.ResetColor();
        }
        catch (ResponseException rex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("=================================================");
            Console.WriteLine("    GREMLIN ERROR");
            Console.WriteLine("=================================================");
            Console.WriteLine($"Message: {rex.Message}");
            Console.WriteLine($"Status Code: {rex.StatusCode}");
            Console.ResetColor();
        }
        catch (FileNotFoundException fnfex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("=================================================");
            Console.WriteLine("    FILE NOT FOUND ERROR");
            Console.WriteLine("=================================================");
            Console.WriteLine($"Message: {fnfex.Message}");
            Console.WriteLine();
            Console.WriteLine("Make sure CSV files exist in the Data directory.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("=================================================");
            Console.WriteLine("    GENERAL ERROR");
            Console.WriteLine("=================================================");
            Console.WriteLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Details: {ex.InnerException.Message}");
            }
            Console.ResetColor();
        }
    }
}