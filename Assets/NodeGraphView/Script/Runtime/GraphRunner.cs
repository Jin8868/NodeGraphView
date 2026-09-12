using System.Collections.Generic;
using System.Linq;

namespace NodeGraphView
{
    public class GraphRunner
    {
        private readonly Dictionary<long, INodeGraphRuntimeNode> nodeMap = new Dictionary<long, INodeGraphRuntimeNode>();
        private INodeGraphRuntimeNode currentNode;

        public string GraphId { get; private set; }
        public NodeGraphRuntimeDocument Document { get; private set; }
        public INodeGraphRuntimeNode CurrentNode => currentNode;
        public bool IsRunning { get; private set; }
        public IReadOnlyCollection<INodeGraphRuntimeNode> Nodes => nodeMap.Values;

        public virtual void Init(NodeGraphRuntimeDocument document)
        {
            Document = document ?? new NodeGraphRuntimeDocument();
            GraphId = string.IsNullOrEmpty(Document.graphId)
                ? GraphUtility.GenerateID().ToString()
                : Document.graphId;
            Document.graphId = GraphId;
            nodeMap.Clear();

            foreach (INodeGraphRuntimeNode node in NodeGraphRuntimeFactory.CreateNodes(this, Document))
            {
                nodeMap[node.NodeId] = node;
            }

            GraphDebugService.RegisterRunner(this);
        }

        public virtual bool Start()
        {
            INodeGraphRuntimeNode startNode = FindStartNode();
            if (startNode == null)
            {
                return false;
            }

            IsRunning = true;
            EnterNode(startNode);
            Step();
            return true;
        }

        public virtual void Step()
        {
            if (!IsRunning || currentNode == null)
            {
                return;
            }

            string exitKey = currentNode.Update();
            if (string.IsNullOrEmpty(exitKey))
            {
                return;
            }

            MoveNext(exitKey);
        }

        public virtual void Update()
        {
            Step();
        }

        public virtual void Stop()
        {
            if (currentNode != null)
            {
                currentNode.OnLeave();
            }

            currentNode = null;
            IsRunning = false;
            GraphDebugService.Clear(GraphId);
        }

        public virtual void Dispose()
        {
            Stop();
            foreach (INodeGraphRuntimeNode node in nodeMap.Values)
            {
                node.OnDispose();
            }

            nodeMap.Clear();
            GraphDebugService.UnregisterRunner(this);
        }

        public bool TryGetNode(long nodeId, out INodeGraphRuntimeNode node)
        {
            return nodeMap.TryGetValue(nodeId, out node);
        }

        protected virtual INodeGraphRuntimeNode FindStartNode()
        {
            return nodeMap.Values.FirstOrDefault(node => node is StartRuntimeNode) ??
                   nodeMap.Values.FirstOrDefault(node => node.Title == "Start");
        }

        public virtual bool MoveNext(string exitKey)
        {
            if (currentNode == null ||
                currentNode.NodeData == null ||
                currentNode.NodeData.nextNodeMap == null ||
                !currentNode.NodeData.nextNodeMap.TryGetValue(exitKey, out long nextNodeId) ||
                !nodeMap.TryGetValue(nextNodeId, out INodeGraphRuntimeNode nextNode))
            {
                Stop();
                return false;
            }

            currentNode.OnLeave();
            EnterNode(nextNode);
            return true;
        }

        protected virtual void EnterNode(INodeGraphRuntimeNode node)
        {
            currentNode = node;
            currentNode.OnEnter();
            GraphDebugService.NotifyCurrentNodeChanged(GraphId, currentNode.NodeId);
        }
    }
}
