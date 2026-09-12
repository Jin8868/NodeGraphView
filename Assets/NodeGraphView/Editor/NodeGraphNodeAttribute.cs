using System;

namespace NodeGraphView.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NodeGraphNodeAttribute : Attribute
    {
        public string MenuName { get; private set; }
        public Type AssetType { get; private set; }
        public Type RuntimeNodeType { get; private set; }

        public NodeGraphNodeAttribute(string menuName, Type assetType)
            : this(menuName, assetType, null)
        {
        }

        public NodeGraphNodeAttribute(string menuName, Type assetType, Type runtimeNodeType)
        {
            MenuName = menuName;
            AssetType = assetType;
            RuntimeNodeType = runtimeNodeType;
        }
    }
}
