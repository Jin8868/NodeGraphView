using System.Collections.Generic;

namespace NodeGraphView
{
    public static class NodeGraphRuntimeExporter
    {
        public static NodeGraphRuntimeDocument CreateRuntimeDocument(NodeGraphDocument editorDocument)
        {
            NodeGraphRuntimeDocument runtimeDocument = new NodeGraphRuntimeDocument();
            if (editorDocument == null)
            {
                return runtimeDocument;
            }

            runtimeDocument.version = editorDocument.version;
            runtimeDocument.graphId = editorDocument.graphId;
            runtimeDocument.graphType = editorDocument.graphType;

            if (editorDocument.nodes != null)
            {
                foreach (NodeGraphNodeData node in editorDocument.nodes)
                {
                    runtimeDocument.nodes.Add(new NodeGraphRuntimeNodeData
                    {
                        nodeId = node.nodeId,
                        nodeType = string.IsNullOrEmpty(node.runtimeNodeType) ? node.nodeType : node.runtimeNodeType,
                        title = node.title,
                        userDataJson = node.userDataJson,
                        nextNodeMap = node.nextNodeMap == null
                            ? new Dictionary<string, long>()
                            : new Dictionary<string, long>(node.nextNodeMap)
                    });
                }
            }

            Dictionary<long, NodeGraphRuntimeNodeData> nodeMap = new Dictionary<long, NodeGraphRuntimeNodeData>();
            foreach (NodeGraphRuntimeNodeData node in runtimeDocument.nodes)
            {
                nodeMap[node.nodeId] = node;
            }

            if (editorDocument.edges != null)
            {
                foreach (NodeGraphEdgeData edge in editorDocument.edges)
                {
                    if (nodeMap.TryGetValue(edge.outputNodeId, out NodeGraphRuntimeNodeData outputNode) &&
                        !outputNode.outputNodeIds.Contains(edge.inputNodeId))
                    {
                        outputNode.outputNodeIds.Add(edge.inputNodeId);
                    }

                    if (nodeMap.TryGetValue(edge.inputNodeId, out NodeGraphRuntimeNodeData inputNode) &&
                        !inputNode.inputNodeIds.Contains(edge.outputNodeId))
                    {
                        inputNode.inputNodeIds.Add(edge.outputNodeId);
                    }
                }
            }

            return runtimeDocument;
        }
    }
}
