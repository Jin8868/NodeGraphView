using System.Collections.Generic;

namespace NodeGraphView
{
    public class NodeGraphNodeData
    {
        public long nodeId;
        public string nodeType;
        public string runtimeNodeType;
        public string viewNodeType;
        public string title;
        public NodeGraphVector2 position;
        public NodeGraphVector2 size;
        public string userDataJson;
        public List<NodeGraphPortData> inputPorts = new List<NodeGraphPortData>();
        public List<NodeGraphPortData> outputPorts = new List<NodeGraphPortData>();
        public Dictionary<string, long> nextNodeMap = new Dictionary<string, long>();
    }
}
