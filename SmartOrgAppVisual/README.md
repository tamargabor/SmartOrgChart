# SmartOrgChart Visualization

Full-stack graph visualization application built with Azure Static Web Apps.

## Project Structure

- **/api** - Azure Functions backend (C# .NET 8)
- **/client** - React frontend (Vite + TypeScript)

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- Azure Cosmos DB with Gremlin API

## Getting Started

### 1. Configure Backend

Navigate to `/api` and update `local.settings.json`:

```json
{
  "Values": {
    "CosmosConnectionString": "AccountEndpoint=https://YOUR_ACCOUNT.gremlin.cosmos.azure.com:443/;AccountKey=YOUR_KEY;Database=OrgDatabase"
  }
}
```

### 2. Build Backend

```bash
cd api
dotnet restore
dotnet build
```

### 3. Install Frontend Dependencies

```bash
cd client
npm install
```

### 4. Run Locally

**Terminal 1 - Backend:**
```bash
cd api
dotnet run
```

The API will run on http://localhost:7071

**Terminal 2 - Frontend:**
```bash
cd client
npm run dev
```

The app will run on http://localhost:5173

## Features

- **Interactive Graph Visualization** - Force-directed graph layout using D3.js
- **Node Coloring** - Different colors for person (blue) and skill (green) nodes
- **Node Details** - Click any node to view its properties in the sidebar
- **Real-time Data** - Fetches live data from Cosmos DB via Gremlin queries

## Deployment

This project is configured for Azure Static Web Apps deployment:

1. Push to GitHub
2. Connect repository to Azure Static Web Apps
3. Set build configuration:
   - **App location:** `/SmartOrgAppVisual/client`
   - **Api location:** `/SmartOrgAppVisual/api`
   - **Output location:** `dist`

## API Endpoints

- `GET /api/graph` - Returns all graph nodes and edges

## Technologies

- **Backend:** Azure Functions, .NET 8, Gremlin.Net
- **Frontend:** React 18, TypeScript, Vite, react-force-graph-2d
- **Database:** Azure Cosmos DB (Gremlin API)
