using System.Collections.Generic;

namespace NodeGraphView
{
    public class NodeGraphRuntimeBlackboard
    {
        private readonly Dictionary<long, NodeGraphRuntimeNodeData> nodeMap = new Dictionary<long, NodeGraphRuntimeNodeData>();

        public List<NodeGraphRuntimeNodeData> nodes = new List<NodeGraphRuntimeNodeData>();

        public NodeGraphRuntimeBlackboard(NodeGraphRuntimeDocument document)
        {
            if (document == null || document.nodes == null)
            {
                return;
            }

            nodes = document.nodes;
            foreach (NodeGraphRuntimeNodeData node in nodes)
            {
                nodeMap[node.nodeId] = node;
            }
        }

        public NodeGraphRuntimeNodeData GetNode(long nodeId)
        {
            nodeMap.TryGetValue(nodeId, out NodeGraphRuntimeNodeData node);
            return node;
        }
    }
}
