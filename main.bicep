// main.bicep - Cost Optimized for Dev/Test
param location string = 'northeurope' 
param accountName string = 'smart-org-graph-${uniqueString(resourceGroup().id)}'
param databaseName string = 'OrgDatabase'
param graphName string = 'OrgGraph'

// 1. Cosmos DB Account (Cost Optimized)
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' = {
  name: accountName
  location: location
  kind: 'GlobalDocumentDB'
  tags: {
    Environment: 'Dev'
    Project: 'SmartOrgChart'
    CostCenter: 'FreeTier'
  }
  properties: {
    // --- COST OPTIMIZATION START ---
    databaseAccountOfferType: 'Standard'
    enableFreeTier: true // The Holy Grail: Free 1000 RU/s + 25GB
    
    // Disable the expensive analytical storage
    enableAnalyticalStorage: false 
    
    // No need for multi-region writes (Multi-master write is expensive)
    enableMultipleWriteLocations: false 

    // Backup strategy: 'Periodic' is the cheapest/default option
    backupPolicy: {
      type: 'Periodic'
      periodicModeProperties: {
        backupIntervalInMinutes: 1440 // Backup every 24 hours
        backupRetentionIntervalInHours: 48 // Retain for 48 hours
      }
    }
    // --- COST OPTIMIZATION END ---

    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false // DISABLED: No zone replication ($$$ savings)
      }
    ]
    capabilities: [
      {
        name: 'EnableGremlin' // Enable graph engine
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session' // Best balance for development
    }
  }
}

// 2. Database
resource database 'Microsoft.DocumentDB/databaseAccounts/gremlinDatabases@2023-11-15' = {
  parent: cosmosAccount
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
  }
}

// 3. Graph (Container)
resource graph 'Microsoft.DocumentDB/databaseAccounts/gremlinDatabases/graphs@2023-11-15' = {
  parent: database
  name: graphName
  properties: {
    resource: {
      id: graphName
      partitionKey: {
        paths: [
          '/partitionKey'
        ]
        kind: 'Hash'
      }
    }
    options: {
      // 400 RU/s is the minimum.
      // Since Free Tier covers up to 1000 RU/s, this costs $0.
      throughput: 400 
    }
  }
}

// 4. Azure Static Web App (Frontend + API)
resource staticWebApp 'Microsoft.Web/staticSites@2022-09-01' = {
  name: 'smart-org-viz-${uniqueString(resourceGroup().id)}'
  location: 'westeurope'
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    repositoryUrl: 'https://github.com/tamargabor/SmartOrgChart'
    branch: 'main'
    provider: 'GitHub'
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
  }
}

// Output: The URL of the website
output webUrl string = staticWebApp.properties.defaultHostname