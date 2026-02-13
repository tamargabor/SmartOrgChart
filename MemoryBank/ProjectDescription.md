# SmartOrgChart - Graph-Based Organizational Visualization

**SmartOrgChart** is a Proof-of-Concept (PoC) solution designed to model and visualize complex organizational structures (employees, skills, projects) using Graph Database technology. It aims to overcome the limitations of traditional relational databases by enabling high-performance traversal of deep relationships (e.g., *"Who knows someone that is an expert in C#?"*).

## 🏗 Architecture Overview

The system is built on **Azure PaaS** (Platform as a Service) components, utilizing a cost-optimized (Free Tier) Serverless architecture.

* **Database:** Azure Cosmos DB for Gremlin (Graph API).
* **Infrastructure:** Infrastructure as Code (IaC) using Azure Bicep.
* **Data Ingestion (ETL):** .NET 8 Console Application (Data seeding & Schema management).
* **Backend API:** Azure Functions (.NET 8 Isolated) - Acts as a secure proxy between the Frontend and the Database.
* **Frontend:** React (Vite + TypeScript) - Interactive visualization using `react-force-graph`.
* **Hosting:** Azure Static Web Apps (SWA).

---

## 📂 Project Structure

| Directory / File | Description |
| :--- | :--- |
| `main.bicep` | Azure Infrastructure definition. Deploys the entire environment (Cosmos DB, SWA) with a single command. |
| **/SmartOrgApp** | **.NET 8 Console App.** The "Writer" layer. Handles CSV import and graph schema logic (Vertex/Edge creation). Uses strongly-typed domain models. |
| **/api** | **Azure Functions.** The "Reader" layer. Exposes a secure endpoint (`/api/graph`) to the frontend, abstracting Cosmos DB credentials. |
| **/client** | **React Web App.** The visualization layer. Built with Vite and TypeScript. |
| `people.csv`, `skills.csv` | Sample datasets for initialization. |

---

## 🚀 Prerequisites

To maintain or redeploy this project in the future, ensure you have the following installed:

1.  **.NET 8 SDK** (or later)
2.  **Node.js LTS** (v20+) and **npm** (Verify in PowerShell: `node -v`)
3.  **Azure CLI** (for deployment)
4.  **Visual Studio 2022** or **VS Code**

---

## 🛠 Deployment & Execution (PowerShell)

### 1. Infrastructure Deployment (Azure)
Open PowerShell in the project root and execute the Bicep deployment:

```powershell
az login
az deployment group create --resource-group <ResourceGroupName> --template-file main.bicep
```

*Note: `main.bicep` is configured to use the **Azure Free Tier** (1000 RU/s free). Limit: 1 per subscription.*

### 2. Data Seeding (Admin Console)
Navigate to the `SmartOrgApp` folder. Update `Program.cs` (or `appsettings.json`) with your new Cosmos DB Endpoint and Primary Key, then run:

```powershell
cd SmartOrgApp
dotnet run
cd ..
```
This command will purge the existing graph and import data from the CSV files.

### 3. Running the Web Application (Local Development)

For local development, it is recommended to open two separate PowerShell terminals.

**Terminal 1: Backend (API)**
```powershell
cd api
# Ensure Cosmos DB connection string is set in local.settings.json or Environment Variables
dotnet run
# (Starts the Function App at http://localhost:7071)
```

**Terminal 2: Frontend (Client)**
```powershell
cd client
npm install  # Install dependencies (first run only)
npm run dev  # Start the development server
```
Once started, open the local URL provided in the terminal (e.g., `http://localhost:5173`) in your browser.

---

## 💡 Critical Technical Notes (Architectural Decisions)

### 1. GraphSON v2 vs. v3 (Protocol Mismatch)
**The Challenge:** Modern `Gremlin.Net` drivers default to GraphSON v3 serialization. However, Azure Cosmos DB currently **only supports GraphSON v2**.
**The Error:** `Gremlin Malformed Request: GraphSON v3 IO is not supported.`
**The Solution:** The generic Gremlin client must be explicitly forced to use the v2 serializer in C#:

```csharp
new GremlinClient(server, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType);
```

### 2. Partition Strategy
* **Requirement:** Partitioning is mandatory in Azure Cosmos DB.
* **Implementation:** We define `/partitionKey` in the Bicep file.
* **Strategy:** Since this is a small-scale organizational graph, we assign a static value (`'org'`) to the partition key for all Vertices and Edges. This ensures all data resides on a single physical partition, enabling efficient **Single Partition Queries** and cheap transactions.

### 3. React + Vite + Security
* **Security:** Client-side code must **never** contain Cosmos DB Master Keys. Access is proxied solely through the Azure Function.
* **Build Tool:** The project uses **Vite** instead of Webpack for faster HMR (Hot Module Replacement). Use `npm run dev` for the development lifecycle.