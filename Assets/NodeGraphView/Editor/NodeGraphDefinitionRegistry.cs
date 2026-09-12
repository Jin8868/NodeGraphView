using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NodeGraphView.Editor
{
    public static class NodeGraphDefinitionRegistry
    {
        private static readonly List<NodeGraphDefinition> Definitions = new List<NodeGraphDefinition>();
        private static bool s_collected;

        public static IReadOnlyList<NodeGraphDefinition> GetDefinitions()
        {
            CollectDefinitions();
            return Definitions;
        }

        private static void CollectDefinitions()
        {
            if (s_collected)
            {
                return;
            }

            s_collected = true;
            Definitions.Clear();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (!typeof(NodeGraphWindow).IsAssignableFrom(type) ||
                            type.IsAbstract ||
                            type.IsGenericType)
                        {
                            continue;
                        }

                        NodeGraphWindowAttribute attribute = type.GetCustomAttribute<NodeGraphWindowAttribute>();
                        if (attribute != null)
                        {
                            Definitions.Add(new NodeGraphDefinition(type, attribute));
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                }
            }

            Definitions.Sort((left, right) => string.Compare(left.Attribute.MenuName, right.Attribute.MenuName, StringComparison.Ordinal));
        }
    }
}
