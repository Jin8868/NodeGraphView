namespace NodeGraphView
{
    public interface INodeGraphRuntimeNode
    {
        long NodeId { get; }
        string Title { get; }
        NodeGraphRuntimeNodeData NodeData { get; }
        void Init(GraphRunner runner, NodeGraphRuntimeNodeData data);
        void OnInit();
        void OnEnter();
        string Update();
        void OnLeave();
        void OnDispose();
    }
}
