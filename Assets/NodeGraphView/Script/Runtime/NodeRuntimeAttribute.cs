using System;

namespace NodeGraphView
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class NodeRuntimeAttribute : Attribute
    {
        public string NodeType { get; private set; }

        public NodeRuntimeAttribute(Type editorNodeType)
        {
            NodeType = editorNodeType == null ? null : editorNodeType.FullName;
        }

        public NodeRuntimeAttribute(string nodeType)
        {
            NodeType = nodeType;
        }
    }
}
