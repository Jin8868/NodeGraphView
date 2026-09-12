using System;

namespace NodeGraphView.Editor
{
    public interface INodeGraphToolbar
    {
        void AddButton(string text, Action clicked);
        void AddSeparator();
    }
}
