# Azure Functions API

Backend API for SmartOrgChart graph visualization.

## Prerequisites
- .NET 8 SDK
- Azure Cosmos DB with Gremlin API

## Local Development

1. Update `local.settings.json` with your Cosmos DB connection string:
```json
{
  "Values": {
    "CosmosConnectionString": "AccountEndpoint=https://YOUR_ACCOUNT.gremlin.cosmos.azure.com:443/;AccountKey=YOUR_KEY;Database=OrgDatabase"
  }
}
```

2. Restore packages and build:
```bash
dotnet restore
dotnet build
```

3. Run locally (requires Azure Functions Core Tools):
```bash
func start
```

Or run with dotnet:
```bash
dotnet run
```

## API Endpoints

### GET /api/graph
Returns all graph nodes and edges in JSON format:
```json
{
  "nodes": [
    {
      "id": "person-id",
      "label": "person",
      "properties": {
        "name": "John Doe",
        "title": "Developer"
      }
    }
  ],
  "links": [
    {
      "source": "person-id",
      "target": "skill-id",
      "label": "hasSkill"
    }
  ]
}
```
