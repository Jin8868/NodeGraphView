using NodeGraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.NodeGraphs.Editor
{
    public class TaskNodeView : NodeViewBase<Game.NodeGraphs.TaskNodeData>
    {
        public override Color GetTitleColor()
        {
            return new Color(0.58f, 0.31f, 0.08f, 1f);
        }

        public override Color GetTitleAccentColor()
        {
            return new Color(1f, 0.63f, 0.18f, 1f);
        }

        public override void Build(VisualElement container)
        {
            TextField nameField = new TextField("Name") { value = Data.name };
            TextField descriptionField = new TextField("Description") { value = Data.description };
            descriptionField.multiline = true;

            nameField.RegisterValueChangedCallback(evt =>
            {
                Data.name = evt.newValue;
                typedNode?.nodeGraphView?.MarkDirty();
            });

            descriptionField.RegisterValueChangedCallback(evt =>
            {
                Data.description = evt.newValue;
                typedNode?.nodeGraphView?.MarkDirty();
            });

            container.Add(nameField);
            container.Add(descriptionField);
        }
    }
}
