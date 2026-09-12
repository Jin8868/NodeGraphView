using NodeGraphView.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeGraphView
{
    public class NodeGraphViewBase : GraphView
    {
        private const int NodeLayer = 0;
        private const int EdgeLayer = 100;
        private readonly List<Port> m_tempCanLinkPort = new List<Port>();
        private Dictionary<Type, string> nodeTypeMap = new Dictionary<Type, string>();

        protected NodeGraphAsset m_graphAsset;
        protected NodeGraphController m_controller;
        protected NodeGraphDocument m_document;
        private bool m_isRebuilding;
        private bool m_isDirty;

        public NodeGraphController Controller => m_controller;
        public NodeGraphAsset GraphAsset => m_graphAsset;
        public bool IsDirty => m_isDirty;
        public event Action<bool> DirtyChanged;

        public virtual void Init(NodeGraphAsset graphAsset)
        {
            m_graphAsset = graphAsset;
            m_controller = NodeGraphControllerRegistry.GetController(graphAsset);
            m_document = m_controller.DeserializeGraph(graphAsset);
            CollectNodeTypes();
            Draw();
        }

        public void SetGraphData(NodeGraphAsset graphAsset)
        {
            m_graphAsset = graphAsset;
            if (m_controller == null)
            {
                m_controller = NodeGraphControllerRegistry.GetController(graphAsset);
            }

            m_document = m_controller.DeserializeGraph(graphAsset);
            CollectNodeTypes();
        }

        public void SetController(NodeGraphController controller)
        {
            m_controller = controller ?? new NodeGraphController();
            CollectNodeTypes();
        }

        protected virtual void Draw()
        {
            AddStyle();
            AddBackGroud();
            AddManipulator();
        }

        public void RebuildGraph()
        {
            if (m_graphAsset == null)
            {
                return;
            }

            m_document = m_controller.DeserializeGraph(m_graphAsset);
            m_isRebuilding = true;
            try
            {
                ClearNodeAndEdge();
                CreateNode(m_document.nodes);
            }
            finally
            {
                m_isRebuilding = false;
            }
        }

        protected virtual void AddStyle()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/NodeGraphView/Editor/NodeGraphWindow.uss");
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }
        }

        private void AddBackGroud()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddManipulator()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContextualMenuManipulator(BuildNodeCreationMenu));
            graphViewChanged += OnGraphViewChanged;
        }

        protected virtual void CollectNodeTypes()
        {
            nodeTypeMap.Clear();
            if (m_graphAsset != null)
            {
                NodeGraphController controller = m_controller ?? NodeGraphControllerRegistry.GetController(m_graphAsset);
                foreach (Type type in controller.GetCreatableNodeTypes(m_graphAsset))
                {
                    nodeTypeMap[type] = controller.GetNodeMenuName(type);
                }
            }

            nodeTypeMap = nodeTypeMap
                .OrderBy(kvp => kvp.Value)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private void BuildNodeCreationMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendSeparator();
            foreach (var type in nodeTypeMap)
            {
                string itemName = $"Create Node/{type.Value}";
                evt.menu.AppendAction(
                    itemName,
                    action => CreateNode(type.Key, action.eventInfo.mousePosition),
                    action => DropdownMenuAction.Status.Normal);
            }
        }

        public void CreateNode(Type nodeType)
        {
            Vector2 localPosition = new Vector2(layout.width * 0.5f, layout.height * 0.5f);
            Vector2 graphPosition = viewTransform.matrix.inverse.MultiplyPoint(localPosition);
            CreateNodeAtGraphPosition(nodeType, graphPosition);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            m_tempCanLinkPort.Clear();
            NodeGraphPortData startPortData = startPort.userData as NodeGraphPortData;
            if (startPortData == null)
            {
                return m_tempCanLinkPort;
            }

            foreach (Port port in ports)
            {
                NodeGraphPortData portData = port.userData as NodeGraphPortData;
                if (portData == null)
                {
                    continue;
                }

                if (startPort.direction == port.direction ||
                    portData.portId == startPortData.portId ||
                    portData.nodeId == startPortData.nodeId)
                {
                    continue;
                }

                if (m_graphAsset != null && m_controller != null && !m_controller.CanConnect(m_graphAsset, startPort, port))
                {
                    continue;
                }

                m_tempCanLinkPort.Add(port);
            }

            return m_tempCanLinkPort;
        }

        protected virtual GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            BringEdgesToFront(graphViewChange.edgesToCreate);
            schedule.Execute(() => BringEdgesToFront(edges)).ExecuteLater(0);

            if (!m_isRebuilding)
            {
                MarkDirty();
            }

            return graphViewChange;
        }

        public void DisconnectPort(Port port)
        {
            foreach (Edge edge in port.connections.ToList())
            {
                edge.input?.Disconnect(edge);
                edge.output?.Disconnect(edge);
                RemoveElement(edge);
            }

            MarkDirty();
        }

        private void CreateNode(Type nodeType, Vector2 screenPosition)
        {
            if (m_graphAsset == null)
            {
                return;
            }

            Vector2 graphPosition = viewTransform.matrix.inverse.MultiplyPoint(screenPosition);
            CreateNodeAtGraphPosition(nodeType, graphPosition);
        }

        protected virtual NodeBase CreateNodeAtGraphPosition(Type nodeType, Vector2 graphPosition)
        {
            if (m_graphAsset == null)
            {
                return null;
            }

            bool isStartNode = typeof(StartNode).IsAssignableFrom(nodeType);
            NodeGraphNodeData nodeData = m_controller.CreateNodeData(m_graphAsset, nodeType, isStartNode);
            nodeData.nodeType = nodeType.FullName;
            nodeData.viewNodeType = nodeType.FullName;
            nodeData.position = graphPosition;
            NodeBase node = CreateNodeView(nodeType, nodeData);
            AddElement(node);
            BringEdgesToFront(edges);
            schedule.Execute(() => BringEdgesToFront(edges)).ExecuteLater(0);
            MarkDirty();
            return node;
        }

        public virtual void ClearNodeAndEdge()
        {
            List<GraphElement> elements = graphElements.ToList();
            foreach (GraphElement element in elements)
            {
                RemoveElement(element);
            }
        }

        public virtual List<NodeBase> CreateNode(List<NodeGraphNodeData> nodeDataList)
        {
            if (nodeDataList == null || nodeDataList.Count <= 0)
            {
                return new List<NodeBase>();
            }

            List<NodeBase> nodeViewList = new List<NodeBase>(nodeDataList.Count);
            foreach (NodeGraphNodeData nodeData in nodeDataList)
            {
                Type nodeType = ResolveNodeType(string.IsNullOrEmpty(nodeData.viewNodeType) ? nodeData.nodeType : nodeData.viewNodeType);
                if (nodeType == null)
                {
                    Debug.LogError($"Node type not found: {nodeData.nodeType}");
                    continue;
                }

                NodeBase newNode = CreateNodeView(nodeType, nodeData);
                nodeViewList.Add(newNode);
                AddElement(newNode);
            }

            LinkEdges();
            return nodeViewList;
        }

        protected virtual NodeBase CreateNodeView(Type nodeType, NodeGraphNodeData nodeData)
        {
            NodeBase newNode = Activator.CreateInstance(nodeType) as NodeBase;
            if (newNode == null)
            {
                newNode = new NodeBase();
            }

            newNode.Init(nodeData, this);
            newNode.layer = NodeLayer;
            return newNode;
        }

        protected virtual void LinkEdges()
        {
            if (m_document == null || m_document.edges == null)
            {
                return;
            }

            List<Edge> linkedEdges = new List<Edge>();
            foreach (NodeGraphEdgeData edgeData in m_document.edges)
            {
                Port outputPort = GetPortByID(edgeData.outputPortId, edgeData.outputNodeId, Direction.Output);
                Port inputPort = GetPortByID(edgeData.inputPortId, edgeData.inputNodeId, Direction.Input);
                if (outputPort == null || inputPort == null)
                {
                    continue;
                }

                Edge edge = outputPort.ConnectTo(inputPort);
                edge.layer = EdgeLayer;
                AddElement(edge);
                BringEdgeToFront(edge);
                linkedEdges.Add(edge);
            }

            BringEdgesToFront(linkedEdges);
            schedule.Execute(() => BringEdgesToFront(linkedEdges)).ExecuteLater(0);
        }

        protected virtual void BringEdgesToFront(IEnumerable<Edge> edgeList)
        {
            if (edgeList == null)
            {
                return;
            }

            foreach (Edge edge in edgeList)
            {
                BringEdgeToFront(edge);
            }
        }

        protected virtual void BringEdgeToFront(Edge edge)
        {
            if (edge == null)
            {
                return;
            }

            edge.layer = EdgeLayer;
            edge.BringToFront();
        }

        protected virtual Port GetPortByID(long portID)
        {
            foreach (Port port in ports)
            {
                if (port.userData is NodeGraphPortData portData && portData.portId == portID)
                {
                    return port;
                }
            }

            return null;
        }

        protected virtual Port GetPortByID(long portID, long nodeID, Direction direction)
        {
            foreach (Port port in ports)
            {
                if (port.direction != direction)
                {
                    continue;
                }

                if (port.userData is NodeGraphPortData portData &&
                    portData.portId == portID &&
                    portData.nodeId == nodeID)
                {
                    return port;
                }
            }

            return null;
        }

        protected virtual Type ResolveNodeType(string fullTypeName)
        {
            Type nodeType = Type.GetType(fullTypeName);
            if (nodeType != null)
            {
                return nodeType;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                nodeType = assembly.GetType(fullTypeName);
                if (nodeType != null)
                {
                    return nodeType;
                }
            }

            return null;
        }

        protected virtual NodeGraphDocument BuildDocument()
        {
            NodeGraphDocument document = new NodeGraphDocument();
            if (m_graphAsset != null)
            {
                document.graphId = m_graphAsset.GraphId;
                document.graphType = m_graphAsset.GetType().FullName;
            }

            foreach (NodeBase node in nodes.ToList().OfType<NodeBase>())
            {
                NodeGraphNodeData nodeData = node.GetSaveData();
                nodeData.nextNodeMap.Clear();
                document.nodes.Add(nodeData);
            }

            document.edges.Clear();
            foreach (Edge edge in edges.ToList())
            {
                if (edge.output?.direction != Direction.Output || edge.input?.direction != Direction.Input)
                {
                    continue;
                }

                if (!(edge.output?.userData is NodeGraphPortData outputData) ||
                    !(edge.input?.userData is NodeGraphPortData inputData))
                {
                    continue;
                }

                NodeGraphNodeData outputNode = document.nodes.FirstOrDefault(node => node.nodeId == outputData.nodeId);
                if (outputNode != null)
                {
                    outputNode.nextNodeMap[outputData.portKey] = inputData.nodeId;
                }

                document.edges.Add(new NodeGraphEdgeData
                {
                    outputNodeId = outputData.nodeId,
                    outputPortId = outputData.portId,
                    inputNodeId = inputData.nodeId,
                    inputPortId = inputData.portId
                });
            }

            return document;
        }

        public void SaveGraph()
        {
            if (m_graphAsset == null || m_controller == null)
            {
                return;
            }

            m_document = BuildDocument();
            m_graphAsset.SetSerializedGraph(m_controller.SerializeGraph(m_graphAsset, m_document));
            EditorUtility.SetDirty(m_graphAsset);
            AssetDatabase.SaveAssets();
            m_isDirty = false;
            DirtyChanged?.Invoke(m_isDirty);
        }

        public void MarkDirty()
        {
            if (m_isRebuilding)
            {
                return;
            }

            if (m_isDirty)
            {
                return;
            }

            m_isDirty = true;
            DirtyChanged?.Invoke(m_isDirty);
        }

        public void ClearDirty()
        {
            if (!m_isDirty)
            {
                return;
            }

            m_isDirty = false;
            DirtyChanged?.Invoke(m_isDirty);
        }

        public virtual void SaveData()
        {
            ExportData();
        }

        public virtual void ExportData()
        {
            if (m_graphAsset == null || m_controller == null)
            {
                Debug.Log("Current graph is null.");
                return;
            }

            NodeGraphDocument document = BuildDocument();
            string data = m_controller.SerializeRuntimeGraph(m_graphAsset, document);
            GraphEditorUnility.SaveDataToFile(
                data,
                m_controller.GetExportFileName(m_graphAsset),
                m_controller.GetExportFolderPath(m_graphAsset),
                m_controller.GetExportExtension(m_graphAsset));
        }

        public virtual void HighlightNode(long nodeId)
        {
            foreach (NodeBase node in nodes.ToList().OfType<NodeBase>())
            {
                node.SetDebugHighlight(node.NodeId == nodeId);
            }
        }

        public virtual void ClearDebugHighlight()
        {
            foreach (NodeBase node in nodes.ToList().OfType<NodeBase>())
            {
                node.SetDebugHighlight(false);
            }
        }

        public virtual void Dispose()
        {
            m_isRebuilding = true;
            try
            {
                ClearNodeAndEdge();
            }
            finally
            {
                m_isRebuilding = false;
            }

            graphViewChanged = null;
            m_tempCanLinkPort.Clear();
        }
    }
}
