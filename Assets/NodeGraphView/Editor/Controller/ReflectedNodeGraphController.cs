using System;
using System.Collections.Generic;
using System.Linq;

namespace NodeGraphView.Editor
{
    public class ReflectedNodeGraphController : NodeGraphController
    {
        private readonly Type m_windowType;
        private readonly NodeGraphWindowAttribute m_attribute;

        public ReflectedNodeGraphController(Type windowType, NodeGraphWindowAttribute attribute)
        {
            m_windowType = windowType;
            m_attribute = attribute;
        }

        public override Type AssetType => m_attribute.AssetType;

        public override Type EditorWindowType => m_windowType;

        public override Type GraphViewType => m_attribute.GraphViewType;

        public override Type DefaultNodeType => m_attribute.DefaultNodeType;

        public override IEnumerable<Type> GetCreatableNodeTypes(NodeGraphAsset asset)
        {
            return GraphEditorUnility.GetNodeTypesForAsset(AssetType)
                .Where(type => type != DefaultNodeType && type != typeof(StartNode));
        }
    }
}
