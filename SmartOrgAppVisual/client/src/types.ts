export interface GraphNode {
    id: string;
    label: string;
    properties: Record<string, any>;
}

export interface GraphLink {
    source: string;
    target: string;
    label: string;
}

export interface GraphData {
    nodes: GraphNode[];
    links: GraphLink[];
}
