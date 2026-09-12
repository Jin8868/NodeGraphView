using System;
using System.Collections.Generic;
using System.Reflection;

namespace NodeGraphView
{
    public static class NodeGraphRuntimeFactory
    {
        private static readonly Dictionary<string, Type> RegisteredNodeTypes = new Dictionary<string, Type>();
        private static bool s_collected;

        public static void RegisterNodeType(string nodeType, Type runtimeNodeType)
        {
            if (string.IsNullOrEmpty(nodeType) ||
                runtimeNodeType == null ||
                !typeof(INodeGraphRuntimeNode).IsAssignableFrom(runtimeNodeType))
            {
                return;
            }

            RegisteredNodeTypes[nodeType] = runtimeNodeType;
        }

        public static List<INodeGraphRuntimeNode> CreateNodes(GraphRunner runner, NodeGraphRuntimeDocument document)
        {
            List<INodeGraphRuntimeNode> nodes = new List<INodeGraphRuntimeNode>();
            if (document == null || document.nodes == null)
            {
                return nodes;
            }

            foreach (NodeGraphRuntimeNodeData nodeData in document.nodes)
            {
                INodeGraphRuntimeNode node = CreateNode(runner, nodeData);
                if (node != null)
                {
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        public static INodeGraphRuntimeNode CreateNode(GraphRunner runner, NodeGraphRuntimeNodeData nodeData)
        {
            if (nodeData == null || string.IsNullOrEmpty(nodeData.nodeType))
            {
                return null;
            }

            Type nodeType = ResolveType(nodeData.nodeType);
            if (nodeType == null || !typeof(INodeGraphRuntimeNode).IsAssignableFrom(nodeType))
            {
                return null;
            }

            INodeGraphRuntimeNode node = Activator.CreateInstance(nodeType) as INodeGraphRuntimeNode;
            node?.Init(runner, nodeData);
            return node;
        }

        private static Type ResolveType(string typeName)
        {
            CollectRegisteredNodeTypes();
            if (RegisteredNodeTypes.TryGetValue(typeName, out Type registeredType))
            {
                return registeredType;
            }

            Type type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static void CollectRegisteredNodeTypes()
        {
            if (s_collected)
            {
                return;
            }

            s_collected = true;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.IsAbstract || !typeof(INodeGraphRuntimeNode).IsAssignableFrom(type))
                        {
                            continue;
                        }

                        foreach (NodeRuntimeAttribute attribute in type.GetCustomAttributes<NodeRuntimeAttribute>())
                        {
                            RegisterNodeType(attribute.NodeType, type);
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                }
            }
        }
    }
}
