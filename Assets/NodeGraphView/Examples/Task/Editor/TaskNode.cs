using NodeGraphView;
using NodeGraphView.Editor;

namespace Game.NodeGraphs.Editor
{
    [NodeGraphNode("Task", typeof(Game.NodeGraphs.TaskGraphAsset), typeof(Game.NodeGraphs.TaskRuntimeNode))]
    public class TaskNode : NodeBase<Game.NodeGraphs.TaskNodeData>
    {
        protected override void InitData()
        {
            base.InitData();
            nodeData.title = "Task";
            EnsurePorts();
        }

        protected override System.Type GetNodeViewType()
        {
            return typeof(TaskNodeView);
        }


        private void EnsurePorts()
        {
            EnsureInputPort("In", "in", "In");
            EnsureOutputPort("Complete", "complete", "Out");
            EnsureOutputPort("Failed", "failed");
            RemoveUnexpectedInputPorts("in");
            RemoveUnexpectedOutputPorts("complete", "failed");
        }
    }
}
