using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gremlin.Net.Driver;
using Gremlin.Net.Structure.IO.GraphSON;

namespace SmartOrgAppVisual;

public class GetGraphData
{
    private readonly ILogger _logger;

    public GetGraphData(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GetGraphData>();
    }

    [Function("GetGraphData")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "graph")] HttpRequestData req)
    {
        _logger.LogInformation("GetGraphData function triggered");

        try
        {
            // Get connection string from environment variables
            var connectionString = Environment.GetEnvironmentVariable("CosmosConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                return await CreateErrorResponse(req, "CosmosConnectionString not configured", HttpStatusCode.InternalServerError);
            }

            // Parse connection string to extract components
            var parts = ParseConnectionString(connectionString);
            
            var gremlinServer = new GremlinServer(
                hostname: parts.Hostname,
                port: parts.Port,
                enableSsl: true,
                username: $"/dbs/{parts.Database}/colls/OrgGraph",
                password: parts.AccountKey
            );

            using var gremlinClient = new GremlinClient(
                gremlinServer,
                new GraphSON2Reader(),
                new GraphSON2Writer(),
                GremlinClient.GraphSON2MimeType
            );

            // Fetch all vertices (nodes)
            var verticesResult = await gremlinClient.SubmitAsync<dynamic>("g.V()");
            var nodes = new List<GraphNode>();

            foreach (var vertex in verticesResult)
            {
                var properties = vertex as IDictionary<string, object>;
                nodes.Add(new GraphNode
                {
                    Id = properties?["id"]?.ToString() ?? "",
                    Label = properties?["label"]?.ToString() ?? "",
                    Properties = ExtractProperties(properties)
                });
            }

            // Fetch all edges (links)
            var edgesResult = await gremlinClient.SubmitAsync<dynamic>("g.E()");
            var links = new List<GraphLink>();

            foreach (var edge in edgesResult)
            {
                var properties = edge as IDictionary<string, object>;
                links.Add(new GraphLink
                {
                    Source = properties?["outV"]?.ToString() ?? "",
                    Target = properties?["inV"]?.ToString() ?? "",
                    Label = properties?["label"]?.ToString() ?? ""
                });
            }

            // Create response
            var graphData = new { nodes, links };
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync(JsonSerializer.Serialize(graphData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching graph data");
            return await CreateErrorResponse(req, $"Error: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    private Dictionary<string, object> ExtractProperties(IDictionary<string, object>? properties)
    {
        var result = new Dictionary<string, object>();
        if (properties == null) return result;

        foreach (var kvp in properties)
        {
            // Skip system properties
            if (kvp.Key == "id" || kvp.Key == "label" || kvp.Key == "type")
                continue;

            // Handle GraphSON2 property format: properties are arrays with [0].value
            if (kvp.Value is IEnumerable<object> array)
            {
                var firstItem = array.FirstOrDefault() as IDictionary<string, object>;
                if (firstItem != null && firstItem.ContainsKey("value"))
                {
                    result[kvp.Key] = firstItem["value"];
                }
            }
            else
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }

    private (string Hostname, int Port, string Database, string AccountKey) ParseConnectionString(string connectionString)
    {
        var parts = connectionString.Split(';')
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToDictionary(
                p => p.Split('=')[0],
                p => string.Join('=', p.Split('=').Skip(1))
            );

        var endpoint = parts["AccountEndpoint"];
        var uri = new Uri(endpoint);
        var hostname = uri.Host;
        var port = uri.Port;
        var database = parts["Database"];
        var accountKey = parts["AccountKey"];

        return (hostname, port, database, accountKey);
    }

    private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, string message, HttpStatusCode statusCode)
    {
        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(new { error = message }));
        return response;
    }
}

public class GraphNode
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class GraphLink
{
    public string Source { get; set; } = "";
    public string Target { get; set; } = "";
    public string Label { get; set; } = "";
}
