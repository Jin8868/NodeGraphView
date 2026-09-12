namespace NodeGraphView
{
    public abstract class NodeGraphRuntimeNodeBase : INodeGraphRuntimeNode
    {
        protected GraphRunner runner;
        protected NodeGraphRuntimeNodeData nodeData;

        public long NodeId => nodeData == null ? 0 : nodeData.nodeId;

        public string Title => nodeData == null ? string.Empty : nodeData.title;

        public NodeGraphRuntimeNodeData NodeData => nodeData;

        public virtual void Init(GraphRunner graphRunner, NodeGraphRuntimeNodeData data)
        {
            runner = graphRunner;
            nodeData = data;
            DeserializeUserData(data == null ? null : data.userDataJson);
            OnInit();
        }

        protected abstract void DeserializeUserData(string userDataJson);

        public virtual void OnInit()
        {
        }

        public virtual void OnEnter()
        {
        }

        public virtual string Update()
        {
            return null;
        }

        public virtual void OnLeave()
        {
        }

        public virtual void OnDispose()
        {
        }
    }
}
