// main.bicep
param location string = resourceGroup().location
param accountName string = 'smart-org-graph-${uniqueString(resourceGroup().id)}' // Egyedi név generálása
param databaseName string = 'OrgDatabase'
param graphName string = 'OrgGraph'

// 1. Create the Cosmos DB Account (with Gremlin API)
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' = {
  name: accountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
        failoverPriority: 0
      }
    ]
    capabilities: [
      {
        name: 'EnableGremlin' // For graph functionality
      }
    ]
  }
}

// 2. Create the Database inside the account
resource database 'Microsoft.DocumentDB/databaseAccounts/gremlinDatabases@2023-11-15' = {
  parent: cosmosAccount
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
  }
}

// 3. Create the Graph itself
resource graph 'Microsoft.DocumentDB/databaseAccounts/gremlinDatabases/graphs@2023-11-15' = {
  parent: database
  name: graphName
  properties: {
    resource: {
      id: graphName
      partitionKey: {
        paths: [
          '/partitionKey' // Scaleable
        ]
        kind: 'Hash'
      }
    }
    options: {
      throughput: 400 // This is the lowest setup. Minimum cost for learning.
    }
  }
}