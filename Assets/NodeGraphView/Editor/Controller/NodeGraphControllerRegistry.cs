using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NodeGraphView.Editor
{
    public static class NodeGraphControllerRegistry
    {
        private static readonly List<NodeGraphController> Controllers = new List<NodeGraphController>();
        private static bool s_collected;

        public static NodeGraphController GetController(NodeGraphAsset asset)
        {
            CollectControllers();
            return Controllers.FirstOrDefault(controller => controller.CanHandle(asset)) ?? new NodeGraphController();
        }

        private static void CollectControllers()
        {
            if (s_collected)
            {
                return;
            }

            s_collected = true;
            Controllers.Clear();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type == typeof(NodeGraphController) ||
                            type == typeof(ReflectedNodeGraphController) ||
                            !typeof(NodeGraphController).IsAssignableFrom(type) ||
                            type.IsAbstract ||
                            type.IsGenericType ||
                            type.GetConstructor(Type.EmptyTypes) == null)
                        {
                            continue;
                        }

                        if (Activator.CreateInstance(type) is NodeGraphController controller)
                        {
                            Controllers.Add(controller);
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                }
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (!typeof(NodeEditorWindowBase).IsAssignableFrom(type) ||
                            type.IsAbstract ||
                            type.IsGenericType)
                        {
                            continue;
                        }

                        NodeGraphWindowAttribute attribute = type.GetCustomAttribute<NodeGraphWindowAttribute>();
                        if (attribute == null || attribute.AssetType == null)
                        {
                            continue;
                        }

                        bool hasExplicitController = Controllers.Any(controller => controller.AssetType == attribute.AssetType);
                        if (!hasExplicitController)
                        {
                            Controllers.Add(new ReflectedNodeGraphController(type, attribute));
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                }
            }

            Controllers.Sort((left, right) => GetInheritanceDepth(right.AssetType).CompareTo(GetInheritanceDepth(left.AssetType)));
            Controllers.Add(new NodeGraphController());
        }

        private static int GetInheritanceDepth(Type type)
        {
            int depth = 0;
            while (type != null)
            {
                depth++;
                type = type.BaseType;
            }

            return depth;
        }
    }
}
