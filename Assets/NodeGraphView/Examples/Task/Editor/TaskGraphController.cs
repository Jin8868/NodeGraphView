using System;
using NodeGraphView;
using NodeGraphView.Editor;

namespace Game.NodeGraphs.Editor
{
    public class TaskGraphController : NodeGraphController
    {
        public override Type AssetType => typeof(Game.NodeGraphs.TaskGraphAsset);
        public override Type EditorWindowType => typeof(TaskGraphWindow);
        public override Type GraphViewType => typeof(NodeGraphView.NodeGraphView);

        public override string GetExportFolderPath(NodeGraphAsset asset)
        {
            return "Assets/Resources/TaskGraphs";
        }
    }
}
