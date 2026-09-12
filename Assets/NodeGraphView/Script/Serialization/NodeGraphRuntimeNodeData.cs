using System.Collections.Generic;

namespace NodeGraphView
{
    public class NodeGraphRuntimeNodeData
    {
        public long nodeId;
        public string nodeType;
        public string title;
        public string userDataJson;
        public Dictionary<string, long> nextNodeMap = new Dictionary<string, long>();
        public List<long> inputNodeIds = new List<long>();
        public List<long> outputNodeIds = new List<long>();
    }
}
