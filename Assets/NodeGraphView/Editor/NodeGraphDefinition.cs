using System;

namespace NodeGraphView.Editor
{
    public class NodeGraphDefinition
    {
        public Type WindowType { get; private set; }
        public NodeGraphWindowAttribute Attribute { get; private set; }

        public NodeGraphDefinition(Type windowType, NodeGraphWindowAttribute attribute)
        {
            WindowType = windowType;
            Attribute = attribute;
        }
    }
}
