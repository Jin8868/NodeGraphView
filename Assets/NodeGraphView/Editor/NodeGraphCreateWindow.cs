using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeGraphView.Editor
{
    public class NodeGraphCreateWindow : EditorWindow
    {
        private void OnEnable()
        {
            rootVisualElement.Clear();

            foreach (NodeGraphDefinition definition in NodeGraphDefinitionRegistry.GetDefinitions())
            {
                Button button = new Button(() => CreateGraph(definition));
                button.text = definition.Attribute.MenuName;
                rootVisualElement.Add(button);
            }
        }

        private void CreateGraph(NodeGraphDefinition definition)
        {
            Type assetType = definition.Attribute.AssetType;
            if (!typeof(NodeGraphAsset).IsAssignableFrom(assetType))
            {
                Debug.LogError($"{assetType.FullName} must inherit NodeGraphAsset.");
                return;
            }

            GraphEditorUnility.CreateGraph(assetType);
            Close();
        }
    }
}
