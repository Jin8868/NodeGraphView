using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeGraphView
{
    [global::NodeGraphView.Editor.NodeGraphNode("Start", typeof(NodeGraphAsset), typeof(StartRuntimeNode))]
    public class StartNode : NodeBase
    {
        protected override Type GetNodeViewType()
        {
            return typeof(StartNodeView);
        }
    }

    public class StartNodeView : NodeViewBase
    {
        public override Color GetTitleColor()
        {
            return new Color(0.14f, 0.42f, 0.23f, 1f);
        }

        public override Color GetTitleAccentColor()
        {
            return new Color(0.42f, 0.92f, 0.54f, 1f);
        }

        public override void Build(VisualElement container)
        {
            VisualElement horizontalContainer = new VisualElement();
            horizontalContainer.style.flexDirection = FlexDirection.Row;
            horizontalContainer.style.alignItems = Align.Center;

            Toggle checkBox = new Toggle();
            checkBox.SetEnabled(false);
            checkBox.value = true;

            Label label = new Label("Start Node");
            horizontalContainer.Add(checkBox);
            horizontalContainer.Add(label);
            container.Add(horizontalContainer);
        }
    }
}
