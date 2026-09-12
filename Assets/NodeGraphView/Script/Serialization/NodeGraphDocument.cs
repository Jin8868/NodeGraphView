using System.Collections.Generic;

namespace NodeGraphView
{
    public class NodeGraphDocument
    {
        public int version = 1;
        public string graphId;
        public string graphType;
        public List<NodeGraphNodeData> nodes = new List<NodeGraphNodeData>();
        public List<NodeGraphEdgeData> edges = new List<NodeGraphEdgeData>();
    }
}
