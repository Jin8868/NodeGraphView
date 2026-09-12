using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;

namespace NodeGraphView.Editor
{
    public class NodeGraphController
    {
        public virtual Type AssetType => typeof(NodeGraphAsset);

        public virtual Type EditorWindowType => typeof(NodeGraphWindow);

        public virtual Type GraphViewType => typeof(NodeGraphView);

        public virtual Type DefaultNodeType => typeof(StartNode);

        public virtual string GetEditorTitle(NodeGraphAsset asset)
        {
            return string.IsNullOrEmpty(asset.graphName) ? asset.name : asset.graphName;
        }

        public virtual string GetExportFileName(NodeGraphAsset asset)
        {
            return string.IsNullOrEmpty(asset.graphName) ? asset.name : asset.graphName;
        }

        public virtual string GetExportFolderPath(NodeGraphAsset asset)
        {
            return "Assets/Resources";
        }

        public virtual string GetExportExtension(NodeGraphAsset asset)
        {
            return ".bytes";
        }

        public virtual IEnumerable<Type> GetCreatableNodeTypes(NodeGraphAsset asset)
        {
            return GraphEditorUnility.GetNodeTypesForAsset(asset.GetType()).Where(type => type != typeof(StartNode));
        }

        public virtual string GetNodeMenuName(Type nodeType)
        {
            NodeGraphNodeAttribute attribute = GraphEditorUnility.GetNodeAttribute(nodeType);
            return attribute == null || string.IsNullOrEmpty(attribute.MenuName)
                ? nodeType.FullName
                : attribute.MenuName;
        }

        public virtual NodeGraphNodeData CreateNodeData(NodeGraphAsset asset, Type nodeViewType, bool isStartNode)
        {
            NodeGraphNodeData nodeData = new NodeGraphNodeData();
            nodeData.nodeId = GenerateUniqueNodeId(asset);
            nodeData.nodeType = nodeViewType.FullName;
            nodeData.title = isStartNode ? "Start" : "New Node";
            nodeData.position = new UnityEngine.Vector2(200, 200);

            if (!isStartNode)
            {
                nodeData.inputPorts.Add(CreatePortData(nodeData.nodeId, "In"));
            }

            nodeData.outputPorts.Add(CreatePortData(nodeData.nodeId, "Out"));
            return nodeData;
        }

        protected virtual long GenerateUniqueNodeId(NodeGraphAsset asset)
        {
            HashSet<long> existingIds = new HashSet<long>();
            NodeGraphDocument document = DeserializeGraph(asset);
            if (document != null && document.nodes != null)
            {
                foreach (NodeGraphNodeData node in document.nodes)
                {
                    existingIds.Add(node.nodeId);
                }
            }

            long nodeId = GraphUtility.GenerateID();
            while (existingIds.Contains(nodeId))
            {
                nodeId = GraphUtility.GenerateID();
            }

            return nodeId;
        }

        public virtual NodeGraphPortData CreatePortData(long nodeId, string portName)
        {
            return new NodeGraphPortData
            {
                portId = GraphUtility.GenerateID(),
                nodeId = nodeId,
                portName = portName,
                portKey = portName
            };
        }

        public virtual bool CanConnect(NodeGraphAsset asset, Port startPort, Port candidatePort)
        {
            return true;
        }

        public virtual NodeGraphDocument DeserializeGraph(NodeGraphAsset asset)
        {
            if (asset == null)
            {
                return new NodeGraphDocument();
            }

            NodeGraphDocument document = GraphRuntimeManager.Instance.Deserialize<NodeGraphDocument>(asset.SerializedGraph);
            if (string.IsNullOrEmpty(document.graphId))
            {
                document.graphId = asset.GraphId;
            }

            if (string.IsNullOrEmpty(document.graphType))
            {
                document.graphType = asset.GetType().FullName;
            }

            return document;
        }

        public virtual string SerializeGraph(NodeGraphAsset asset, NodeGraphDocument document)
        {
            return GraphRuntimeManager.Instance.Serialize(document);
        }

        public virtual string SerializeRuntimeGraph(NodeGraphAsset asset, NodeGraphDocument document)
        {
            NodeGraphRuntimeDocument runtimeDocument = NodeGraphRuntimeExporter.CreateRuntimeDocument(document);
            return GraphRuntimeManager.Instance.Serialize(runtimeDocument);
        }

        public virtual void BuildToolbar(NodeGraphAsset asset, INodeGraphToolbar toolbar, NodeEditorWindowBase window)
        {
        }

        public virtual bool CanHandle(NodeGraphAsset asset)
        {
            return asset != null && AssetType.IsAssignableFrom(asset.GetType());
        }
    }
}
