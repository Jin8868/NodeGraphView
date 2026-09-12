using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NodeGraphView.Editor
{
    public class NodeGraphToolbar : INodeGraphToolbar
    {
        private readonly Toolbar m_toolbar;

        public NodeGraphToolbar(Toolbar toolbar)
        {
            m_toolbar = toolbar;
        }

        public void AddButton(string text, Action clicked)
        {
            Button button = new Button();
            button.text = text;
            button.clicked += clicked;
            m_toolbar.Add(button);
        }

        public void AddSeparator()
        {
            ToolbarSpacer spacer = new ToolbarSpacer();
            m_toolbar.Add(spacer);
        }
    }
}
