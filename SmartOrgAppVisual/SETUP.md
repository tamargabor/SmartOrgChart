# Setup Instructions for SmartOrgChart Visualization

## Prerequisites

Before running the application, you need to install:

1. **.NET 8 SDK** - Download from https://dotnet.microsoft.com/download/dotnet/8.0
2. **Node.js 18+** - Download from https://nodejs.org/ (LTS version recommended)
3. **Azure Functions Core Tools** (optional, for local debugging):
   ```powershell
   npm install -g azure-functions-core-tools@4
   ```

## Step-by-Step Setup

### 1. Update Connection String

Navigate to `/SmartOrgAppVisual/api/` and copy the actual Cosmos DB connection string:

**File: `SmartOrgAppVisual/api/local.settings.json`**

Replace `YOUR_KEY_HERE` with your actual Cosmos DB key from `appsettings.Development.json`.

### 2. Install Backend Dependencies

```powershell
cd SmartOrgAppVisual\api
dotnet restore
dotnet build
```

### 3. Install Frontend Dependencies

```powershell
cd ..\client
npm install
```

This will install:
- React 18
- react-force-graph-2d
- Vite
- TypeScript
- All dependencies from package.json

### 4. Run the Application

You need to run both backend and frontend simultaneously in separate terminals.

**Terminal 1 - Backend API:**
```powershell
cd SmartOrgAppVisual\api
dotnet run
```

The API will start on: http://localhost:7071
Endpoint available: http://localhost:7071/api/graph

**Terminal 2 - Frontend:**
```powershell
cd SmartOrgAppVisual\client
npm run dev
```

The app will start on: http://localhost:5173

### 5. Open in Browser

Navigate to http://localhost:5173 and you should see the interactive graph visualization!

## Troubleshooting

**TypeScript errors in editor?**
- These are expected until you run `npm install` in the `/client` folder
- The errors will disappear after dependencies are installed

**API connection failed?**
- Make sure the backend is running on port 7071
- Check that your Cosmos DB connection string is correct in `local.settings.json`
- Verify your Cosmos DB is accessible and contains data

**Graph not displaying?**
- Check browser console for errors (F12)
- Verify the API returns data: http://localhost:7071/api/graph
- Ensure your graph database has vertices and edges

## Features

- **Interactive Force Graph**: Drag nodes, zoom, and pan
- **Color-coded Nodes**:
  - Blue = Person
  - Green = Skill
- **Click to Explore**: Click any node to see its properties
- **Real-time Data**: Fetches live data from Cosmos DB

## Next Steps

To deploy to Azure:
1. Push to GitHub
2. Create Azure Static Web App resource
3. Connect to your repository
4. Configure build settings (see `/SmartOrgAppVisual/README.md`)
