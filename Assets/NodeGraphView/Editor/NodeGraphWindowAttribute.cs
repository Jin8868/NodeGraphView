using System;

namespace NodeGraphView.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NodeGraphWindowAttribute : Attribute
    {
        public string MenuName { get; private set; }
        public Type AssetType { get; private set; }
        public Type GraphViewType { get; private set; }
        public Type DefaultNodeType { get; private set; }

        public NodeGraphWindowAttribute(string menuName, Type assetType)
            : this(menuName, assetType, typeof(NodeGraphView), typeof(StartNode))
        {
        }

        public NodeGraphWindowAttribute(string menuName, Type assetType, Type graphViewType, Type defaultNodeType)
        {
            MenuName = menuName;
            AssetType = assetType;
            GraphViewType = graphViewType;
            DefaultNodeType = defaultNodeType;
        }
    }
}
