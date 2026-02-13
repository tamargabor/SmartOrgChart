import { useEffect, useState, useCallback } from 'react';
import ForceGraph2D from 'react-force-graph-2d';
import type { GraphData, GraphNode } from './types';
import './App.css';

interface ForceGraphNode extends GraphNode {
  x?: number;
  y?: number;
  vx?: number;
  vy?: number;
}

interface ForceGraphLink {
  source: string | ForceGraphNode;
  target: string | ForceGraphNode;
  label: string;
}

interface ForceGraphData {
  nodes: ForceGraphNode[];
  links: ForceGraphLink[];
}

function App() {
  const [graphData, setGraphData] = useState<ForceGraphData | null>(null);
  const [selectedNode, setSelectedNode] = useState<ForceGraphNode | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchGraphData();
  }, []);

  const fetchGraphData = async () => {
    try {
      setLoading(true);
      const response = await fetch('/api/graph');

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data: GraphData = await response.json();
      setGraphData(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch graph data');
      console.error('Error fetching graph data:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleNodeClick = useCallback((node: ForceGraphNode) => {
    setSelectedNode(node);
  }, []);

  const getNodeColor = (node: ForceGraphNode) => {
    switch (node.label.toLowerCase()) {
      case 'person':
        return '#4a90e2'; // Blue for persons
      case 'skill':
        return '#7cb342'; // Green for skills
      default:
        return '#9e9e9e'; // Gray for others
    }
  };

  const getNodeLabel = (node: ForceGraphNode) => {
    // Try to get a display name from properties
    const name = node.properties?.name ||
      node.properties?.skillName ||
      node.properties?.title ||
      node.id;
    return name;
  };

  if (loading) {
    return (
      <div className="app-container">
        <div className="loading">Loading graph data...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="app-container">
        <div className="error">
          <h2>Error</h2>
          <p>{error}</p>
          <button onClick={fetchGraphData}>Retry</button>
        </div>
      </div>
    );
  }

  if (!graphData) {
    return (
      <div className="app-container">
        <div className="error">No graph data available</div>
      </div>
    );
  }

  return (
    <div className="app-container">
      <header className="app-header">
        <h1>SmartOrgChart - Graph Visualization</h1>
        <div className="stats">
          <span>Nodes: {graphData.nodes.length}</span>
          <span>Links: {graphData.links.length}</span>
        </div>
      </header>

      <div className="main-content">
        <div className="graph-container">
          <ForceGraph2D
            graphData={graphData}
            nodeLabel={getNodeLabel}
            nodeColor={getNodeColor}
            nodeRelSize={6}
            linkDirectionalArrowLength={3.5}
            linkDirectionalArrowRelPos={1}
            linkColor={() => '#666666'}
            onNodeClick={handleNodeClick}
            nodeCanvasObject={(node: any, ctx: CanvasRenderingContext2D, globalScale: number) => {
              const label = getNodeLabel(node);
              const fontSize = 12 / globalScale;
              ctx.font = `${fontSize}px Sans-Serif`;

              // Draw node circle
              ctx.beginPath();
              ctx.arc(node.x!, node.y!, 6, 0, 2 * Math.PI);
              ctx.fillStyle = getNodeColor(node);
              ctx.fill();

              // Draw label
              ctx.textAlign = 'center';
              ctx.textBaseline = 'middle';
              ctx.fillStyle = '#ffffff';
              ctx.fillText(label, node.x!, node.y! + 12);
            }}
          />
        </div>

        {selectedNode && (
          <div className="sidebar">
            <div className="sidebar-header">
              <h2>Node Details</h2>
              <button onClick={() => setSelectedNode(null)}>×</button>
            </div>
            <div className="sidebar-content">
              <div className="detail-row">
                <span className="label">ID:</span>
                <span className="value">{selectedNode.id}</span>
              </div>
              <div className="detail-row">
                <span className="label">Type:</span>
                <span
                  className="value badge"
                  style={{ backgroundColor: getNodeColor(selectedNode) }}
                >
                  {selectedNode.label}
                </span>
              </div>

              <h3>Properties</h3>
              {Object.entries(selectedNode.properties).length > 0 ? (
                <div className="properties">
                  {Object.entries(selectedNode.properties).map(([key, value]) => (
                    <div key={key} className="detail-row">
                      <span className="label">{key}:</span>
                      <span className="value">{String(value)}</span>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="no-properties">No additional properties</p>
              )}
            </div>
          </div>
        )}
      </div>

      <div className="legend">
        <div className="legend-item">
          <span className="legend-color" style={{ backgroundColor: '#4a90e2' }}></span>
          Person
        </div>
        <div className="legend-item">
          <span className="legend-color" style={{ backgroundColor: '#7cb342' }}></span>
          Skill
        </div>
      </div>
    </div>
  );
}

export default App;
