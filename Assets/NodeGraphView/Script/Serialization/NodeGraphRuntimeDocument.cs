using System.Collections.Generic;

namespace NodeGraphView
{
    public class NodeGraphRuntimeDocument
    {
        public int version = 1;
        public string graphId;
        public string graphType;
        public List<NodeGraphRuntimeNodeData> nodes = new List<NodeGraphRuntimeNodeData>();
    }
}
