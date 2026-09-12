using UnityEngine;
using UnityEngine.UIElements;

namespace NodeGraphView
{
    public abstract class NodeViewBase
    {
        protected NodeBase node;

        public virtual void Init(NodeBase owner)
        {
            node = owner;
        }

        public virtual void Build(VisualElement container)
        {
        }

        public virtual void ApplyVisualStyle()
        {
        }

        public virtual Color GetTitleColor()
        {
            return new Color(0.1f, 0.34f, 0.55f, 1f);
        }

        public virtual Color GetTitleAccentColor()
        {
            return new Color(0.43f, 0.78f, 1f, 1f);
        }
    }

    public abstract class NodeViewBase<TData> : NodeViewBase where TData : new()
    {
        protected NodeBase<TData> typedNode;

        public override void Init(NodeBase owner)
        {
            base.Init(owner);
            typedNode = owner as NodeBase<TData>;
        }

        protected TData Data => typedNode == null ? new TData() : typedNode.UserData;
    }
}
