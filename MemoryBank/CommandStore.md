# 1. Login (Follow the instructions in the browser)
az login

# 2. Create Resource Group
# Create a logical container for resources
az group create --name SmartOrgChartRG --location westeurope

# 3. Run the Bicep file (Deployment)
# This creates the Cosmos DB based on the specification
az deployment group create --resource-group SmartOrgChartRG --template-file main.bicep



# 1. Endpoint (Server address) - Look for the "gremlinEndpoint" value!
az cosmosdb show --name smart-org-graph-zujg5wprz76ua --resource-group SmartOrgChartRG --query documentEndpoint --output tsv

# 2. Primary Key (The password)
az cosmosdb keys list --name smart-org-graph-zujg5wprz76ua --resource-group SmartOrgChartRG --query primaryMasterKey --output tsv

# Run client - in client folder
npm run dev

# Run API - in api folder
func start
