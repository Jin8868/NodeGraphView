using UnityEditor;

namespace NodeGraphView.Editor
{
    [NodeGraphWindow("Default Graph", typeof(NodeGraphAsset))]
    public class NodeGraphWindow : NodeEditorWindowBase
    {
        public static new NodeGraphWindow Open(NodeGraphAsset nodeGraph)
        {
            return NodeEditorWindowBase.Open(nodeGraph) as NodeGraphWindow;
        }
    }
}
