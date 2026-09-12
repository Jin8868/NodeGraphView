using NodeGraphView;
using NodeGraphView.Editor;
using UnityEditor;

namespace Game.NodeGraphs.Editor
{
    [NodeGraphWindow("Task Graph", typeof(Game.NodeGraphs.TaskGraphAsset), typeof(NodeGraphView.NodeGraphView), typeof(NodeGraphView.StartNode))]
    public class TaskGraphWindow : NodeEditorWindowBase
    {
        [MenuItem("Tools/Node Graph/Task/Create Task Graph")]
        public static void CreateGraph()
        {
            GraphEditorUnility.CreateGraph<Game.NodeGraphs.TaskGraphAsset>();
        }

        public override void BuildToolbar(INodeGraphToolbar toolbar)
        {
            toolbar.AddButton("Task", CreateTaskNode);
        }

        private void CreateTaskNode()
        {
            m_graphView?.CreateNode(typeof(TaskNode));
        }

    }
}
