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
    public class NodeBase : Node
    {
        public NodeGraphViewBase nodeGraphView { get; private set; }

        public NodeGraphNodeData nodeData;

        public long NodeId => nodeData != null ? nodeData.nodeId : 0;

        private bool isResizing;
        private ResizeDirection resizeDirection;
        private Vector2 resizeStartPosition;
        private Rect resizeStartRect;
        private bool resizeStartMovable;
        private bool resizeHoverDisabledMovable;
        private bool resizeCallbacksRegistered;
        private VisualElement resizeCaptureTarget;

        protected VisualElement resizeHandle;
        protected List<VisualElement> resizeHandles = new List<VisualElement>();
        protected VisualElement titleAccent;
        protected Label nodeIdLabel;
        protected NodeViewBase nodeView;
        protected Vector2 minSize = new Vector2(185, 80);
        protected Vector2 maxSize = new Vector2(800, 600);
        protected float resizeEdgeSize = 10f;

        private StyleColor m_originColor;

        public virtual void Init(NodeGraphNodeData data, NodeGraphViewBase graphView)
        {
            nodeGraphView = graphView;
            nodeData = data ?? new NodeGraphNodeData();
            EnsureDefaultData();
            InitData();
            InitComponent();
            Refresh();
        }

        public virtual void Init(NodeGraphViewBase graphView, bool isStartNode)
        {
            Init(GraphEditorUnility.CreateDefaultNodeData(isStartNode), graphView);
        }

        protected virtual void InitData()
        {
            m_originColor = titleContainer.style.backgroundColor;
        }

        protected virtual void InitComponent()
        {
            AddStyle();
            ConfigureResizableLayout();
            Vector2 nodeSize = nodeData.size;
            Rect nodeRect = nodeSize.x > 0 && nodeSize.y > 0
                ? new Rect(nodeData.position, nodeSize)
                : new Rect(nodeData.position, Vector2.zero);
            SetPosition(nodeRect);
            InitTitleLabel();
            InitNodeIdLabel();
            InitNodeView();
            ApplyVisualStyle();
            InitPort();
            BuildContent();
            AddResizeHandle();
        }

        protected virtual void ConfigureResizableLayout()
        {
            AddToClassList("ng-node");
            mainContainer.AddToClassList("ng-node-main");
            titleContainer.AddToClassList("ng-node-title");
            inputContainer.AddToClassList("ng-node-inputs");
            outputContainer.AddToClassList("ng-node-outputs");
            topContainer.AddToClassList("ng-node-port-row");

            style.flexDirection = FlexDirection.Column;

            mainContainer.style.flexGrow = 1;
            inputContainer.style.flexGrow = 1;
            outputContainer.style.flexGrow = 1;
            inputContainer.style.flexBasis = 0;
            outputContainer.style.flexBasis = 0;

            extensionContainer.style.flexGrow = 1;
            extensionContainer.style.minHeight = 0;
        }

        protected virtual void InitTitleLabel()
        {
            titleAccent = new VisualElement();
            titleAccent.AddToClassList("ng-node-title-accent");
            titleContainer.Insert(0, titleAccent);

            TextField textField = new TextField
            {
                value = nodeData.title
            };
            textField.AddToClassList("ng-node-title-field");

            textField.RegisterValueChangedCallback(evt =>
            {
                nodeData.title = evt.newValue;
                nodeGraphView.MarkDirty();
            });

            titleContainer.Insert(1, textField);
        }

        protected virtual void InitNodeIdLabel()
        {
            VisualElement nodeIdRow = new VisualElement();
            nodeIdRow.AddToClassList("node-id-row");

            nodeIdLabel = new Label($"ID: {NodeId}");
            nodeIdLabel.AddToClassList("node-id-label");
            nodeIdLabel.pickingMode = PickingMode.Ignore;

            Button copyIdButton = new Button(() =>
            {
                EditorGUIUtility.systemCopyBuffer = NodeId.ToString();
            })
            {
                text = "Copy ID"
            };
            copyIdButton.AddToClassList("node-id-copy-button");

            nodeIdRow.Add(nodeIdLabel);
            nodeIdRow.Add(copyIdButton);
            mainContainer.Insert(1, nodeIdRow);
        }

        protected virtual void BuildContent()
        {
            nodeView?.Build(extensionContainer);
        }

        public virtual void InitPort()
        {
            foreach (NodeGraphPortData port in nodeData.inputPorts)
            {
                InstantiatePort(port, Direction.Input, inputContainer);
            }

            foreach (NodeGraphPortData port in nodeData.outputPorts)
            {
                InstantiatePort(port, Direction.Output, outputContainer);
            }
        }

        protected virtual void AddStyle()
        {
            string path = GetStyleSheetPath();
            StyleSheet styleSheet = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }

            extensionContainer.AddToClassList("extension-container");
        }

        protected virtual string GetStyleSheetPath()
        {
            return "Assets/NodeGraphView/Editor/NodeGraphWindow.uss";
        }

        protected virtual void ApplyVisualStyle()
        {
            titleContainer.style.backgroundColor = nodeView == null
                ? new Color(0.1f, 0.34f, 0.55f, 1f)
                : nodeView.GetTitleColor();
            if (titleAccent != null)
            {
                titleAccent.style.backgroundColor = nodeView == null
                    ? new Color(0.43f, 0.78f, 1f, 1f)
                    : nodeView.GetTitleAccentColor();
            }

            nodeView?.ApplyVisualStyle();
        }

        protected virtual void InitNodeView()
        {
            Type viewType = GetNodeViewType();
            if (viewType == null)
            {
                return;
            }

            nodeView = Activator.CreateInstance(viewType) as NodeViewBase;
            nodeView?.Init(this);
        }

        protected virtual Type GetNodeViewType()
        {
            return null;
        }

        public virtual object GetUserDataObject()
        {
            return null;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Copy Node ID", _ => CopyNodeIdToClipboard());
        }

        protected virtual void CopyNodeIdToClipboard()
        {
            EditorGUIUtility.systemCopyBuffer = NodeId.ToString();
        }

        protected virtual Button AddButtonToContainer(string text, Action clickCallBack, VisualElement container, int index)
        {
            if (container == null)
            {
                return null;
            }

            Button newBtn = new Button();
            newBtn.text = text;
            newBtn.clicked += clickCallBack;
            container.Insert(index, newBtn);
            return newBtn;
        }

        public virtual void AddOutPutPort()
        {
            AddOutputPort();
        }

        public virtual NodeGraphPortData AddOutputPort(string portName = "Out", string portKey = null)
        {
            NodeGraphPortData portData = CreatePortData(portName, portKey);
            nodeData.outputPorts.Add(portData);
            InstantiatePort(portData, Direction.Output, outputContainer);
            RefreshExpandedState();
            nodeGraphView?.MarkDirty();
            return portData;
        }

        public virtual void AddInputPort()
        {
            AddInputPort("In");
        }

        public virtual NodeGraphPortData AddInputPort(string portName = "In", string portKey = null)
        {
            NodeGraphPortData portData = CreatePortData(portName, portKey);
            nodeData.inputPorts.Add(portData);
            InstantiatePort(portData, Direction.Input, inputContainer);
            RefreshExpandedState();
            nodeGraphView?.MarkDirty();
            return portData;
        }

        protected NodeGraphPortData EnsureInputPort(string portName, string portKey, params string[] legacyPortKeys)
        {
            return EnsurePort(nodeData.inputPorts, portName, portKey, legacyPortKeys);
        }

        protected NodeGraphPortData EnsureOutputPort(string portName, string portKey, params string[] legacyPortKeys)
        {
            return EnsurePort(nodeData.outputPorts, portName, portKey, legacyPortKeys);
        }

        protected NodeGraphPortData EnsurePort(
            List<NodeGraphPortData> ports,
            string portName,
            string portKey,
            params string[] legacyPortKeys)
        {
            NodeGraphPortData foundPort = null;
            foreach (NodeGraphPortData port in ports)
            {
                if (IsPortKeyMatch(port.portKey, portKey))
                {
                    foundPort = port;
                    break;
                }
            }

            if (foundPort == null)
            {
                foreach (NodeGraphPortData port in ports)
                {
                    if (IsPortKeyMatch(port.portKey, legacyPortKeys))
                    {
                        foundPort = port;
                        break;
                    }
                }
            }

            if (foundPort == null)
            {
                foundPort = CreatePortData(portName, portKey);
                ports.Add(foundPort);
            }

            foundPort.nodeId = NodeId;
            foundPort.portName = portName;
            foundPort.portKey = portKey;
            return foundPort;
        }

        protected void RemoveUnexpectedInputPorts(params string[] allowedPortKeys)
        {
            RemoveUnexpectedPorts(nodeData.inputPorts, allowedPortKeys);
        }

        protected void RemoveUnexpectedOutputPorts(params string[] allowedPortKeys)
        {
            RemoveUnexpectedPorts(nodeData.outputPorts, allowedPortKeys);
        }

        protected void RemoveUnexpectedPorts(List<NodeGraphPortData> ports, params string[] allowedPortKeys)
        {
            HashSet<string> keptPortKeys = new HashSet<string>();
            for (int i = 0; i < ports.Count;)
            {
                string portKey = ports[i].portKey;
                if (!IsPortKeyMatch(portKey, allowedPortKeys) || keptPortKeys.Contains(portKey))
                {
                    RemovePortElementByID(ports[i].portId);
                    ports.RemoveAt(i);
                    continue;
                }

                keptPortKeys.Add(portKey);
                i++;
            }
        }

        protected virtual NodeGraphPortData CreatePortData(string portName, string portKey = null)
        {
            return new NodeGraphPortData
            {
                portId = GraphUtility.GenerateID(),
                nodeId = NodeId,
                portName = portName,
                portKey = string.IsNullOrEmpty(portKey) ? portName : portKey
            };
        }

        private bool IsPortKeyMatch(string portKey, params string[] candidates)
        {
            foreach (string candidate in candidates)
            {
                if (string.Equals(portKey, candidate, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual Port InstantiatePort(NodeGraphPortData portData, Direction direction, VisualElement portContainer)
        {
            VisualElement horizontalContainer = new VisualElement();
            horizontalContainer.AddToClassList("ng-port-wrapper");
            horizontalContainer.AddToClassList(direction == Direction.Input ? "ng-port-wrapper-input" : "ng-port-wrapper-output");
            horizontalContainer.style.flexDirection = direction == Direction.Input ? FlexDirection.Row : FlexDirection.RowReverse;
            horizontalContainer.style.alignItems = Align.Center;
            horizontalContainer.style.justifyContent = direction == Direction.Input ? Justify.FlexStart : Justify.FlexEnd;
            horizontalContainer.style.flexGrow = 1;
            horizontalContainer.style.flexShrink = 1;
            horizontalContainer.style.width = Length.Percent(100);

            Port newPort = base.InstantiatePort(Orientation.Horizontal, direction, Port.Capacity.Multi, typeof(bool));
            newPort.name = string.Empty;
            newPort.portName = portData.portName;
            newPort.userData = portData;
            newPort.AddToClassList(direction == Direction.Input ? "ng-port-input" : "ng-port-output");
            newPort.style.flexDirection = FlexDirection.Row;
            newPort.style.flexGrow = 1;
            newPort.style.flexShrink = 1;
            newPort.style.width = Length.Percent(100);
            newPort.style.justifyContent = direction == Direction.Input ? Justify.FlexStart : Justify.FlexEnd;

            VisualElement connector = newPort.Q<VisualElement>("connector");
            if (connector != null)
            {
                newPort.style.position = Position.Relative;
                connector.style.position = Position.Absolute;
                if (direction == Direction.Input)
                {
                    connector.style.left = 0;
                    connector.style.right = StyleKeyword.Auto;
                }
                else
                {
                    connector.style.left = StyleKeyword.Auto;
                    connector.style.right = 0;
                }
            }

            Label portLabel = newPort.Q<Label>();
            if (portLabel != null)
            {
                portLabel.style.flexGrow = 0;
                portLabel.style.flexShrink = 0;
                if (direction == Direction.Input)
                {
                    portLabel.style.marginLeft = 16;
                }
                else
                {
                    portLabel.style.marginRight = 16;
                }
            }

            horizontalContainer.Add(newPort);
            portContainer.Add(horizontalContainer);
            return newPort;
        }

        public virtual NodeGraphNodeData GetSaveData()
        {
            Rect rect = GetPosition();
            nodeData.position = rect.position;
            nodeData.size = GetCurrentSize(rect);
            nodeData.nodeType = GetType().FullName;
            nodeData.viewNodeType = GetType().FullName;
            nodeData.runtimeNodeType = GetRuntimeNodeTypeName();
            nodeData.userDataJson = SerializeUserData();
            EnsureDefaultData();
            return nodeData;
        }

        protected virtual string SerializeUserData()
        {
            return nodeData.userDataJson;
        }

        protected virtual string GetRuntimeNodeTypeName()
        {
            NodeGraphNodeAttribute attribute = GraphEditorUnility.GetNodeAttribute(GetType());
            if (attribute != null && attribute.RuntimeNodeType != null)
            {
                return attribute.RuntimeNodeType.FullName;
            }

            return GetType().FullName;
        }

        protected virtual void EnsureDefaultData()
        {
            if (nodeData.nodeId <= 0)
            {
                nodeData.nodeId = GraphUtility.GenerateID();
            }

            if (string.IsNullOrEmpty(nodeData.nodeType))
            {
                nodeData.nodeType = GetType().FullName;
            }

            if (string.IsNullOrEmpty(nodeData.runtimeNodeType))
            {
                nodeData.runtimeNodeType = GetRuntimeNodeTypeName();
            }

            if (string.IsNullOrEmpty(nodeData.viewNodeType))
            {
                nodeData.viewNodeType = GetType().FullName;
            }

            if (string.IsNullOrEmpty(nodeData.title))
            {
                nodeData.title = "New Node";
            }

            if (nodeData.inputPorts == null)
            {
                nodeData.inputPorts = new List<NodeGraphPortData>();
            }

            if (nodeData.outputPorts == null)
            {
                nodeData.outputPorts = new List<NodeGraphPortData>();
            }

            if (nodeData.nextNodeMap == null)
            {
                nodeData.nextNodeMap = new Dictionary<string, long>();
            }

            EnsurePortKeys(nodeData.inputPorts);
            EnsurePortKeys(nodeData.outputPorts);
        }

        private void EnsurePortKeys(List<NodeGraphPortData> ports)
        {
            foreach (NodeGraphPortData port in ports)
            {
                if (string.IsNullOrEmpty(port.portKey))
                {
                    port.portKey = string.IsNullOrEmpty(port.portName) ? port.portId.ToString() : port.portName;
                }
            }
        }

        protected Port GetPortByID(long portID)
        {
            return this.Query<Port>().ToList()
                .FirstOrDefault(port => port.userData is NodeGraphPortData portData && portData.portId == portID);
        }

        public void RemovePortByID(long portID)
        {
            RemovePortFromList(nodeData.inputPorts, portID);
            RemovePortFromList(nodeData.outputPorts, portID);
        }

        private void RemovePortFromList(List<NodeGraphPortData> ports, long portID)
        {
            for (int i = ports.Count - 1; i >= 0; i--)
            {
                if (ports[i].portId != portID)
                {
                    continue;
                }

                RemovePortElementByID(portID);
                ports.RemoveAt(i);
                return;
            }
        }

        private void RemovePortElementByID(long portID)
        {
            Port port = GetPortByID(portID);
            if (port == null)
            {
                return;
            }

            nodeGraphView?.DisconnectPort(port);
            if (port.parent != null)
            {
                port.parent.RemoveFromHierarchy();
            }
        }

        public virtual void Refresh()
        {
            RefreshExpandedState();
        }

        protected virtual void AddResizeHandle()
        {
            resizeHandles.Clear();
            AddResizeHandle(ResizeDirection.Left, "resize-handle-left");
            AddResizeHandle(ResizeDirection.Right, "resize-handle-right");
            AddResizeHandle(ResizeDirection.Top, "resize-handle-top");
            AddResizeHandle(ResizeDirection.Bottom, "resize-handle-bottom");
            AddResizeHandle(ResizeDirection.Top | ResizeDirection.Left, "resize-handle-top-left");
            AddResizeHandle(ResizeDirection.Top | ResizeDirection.Right, "resize-handle-top-right");
            AddResizeHandle(ResizeDirection.Bottom | ResizeDirection.Left, "resize-handle-bottom-left");
            resizeHandle = AddResizeHandle(ResizeDirection.Bottom | ResizeDirection.Right, "resize-handle-bottom-right");
        }

        private void RegisterResizeCallbacks()
        {
            if (resizeCallbacksRegistered)
            {
                return;
            }

            resizeCallbacksRegistered = true;
            RegisterCallback<MouseDownEvent>(OnNodeResizeMouseDown, TrickleDown.TrickleDown);
            RegisterCallback<MouseMoveEvent>(OnNodeResizeMouseMove, TrickleDown.TrickleDown);
            RegisterCallback<MouseUpEvent>(OnNodeResizeMouseUp, TrickleDown.TrickleDown);
            RegisterCallback<MouseLeaveEvent>(OnNodeResizeMouseLeave);
        }

        private VisualElement AddResizeHandle(ResizeDirection direction, string className)
        {
            VisualElement handle = new VisualElement();
            handle.style.position = Position.Absolute;
            handle.style.backgroundColor = Color.clear;
            handle.pickingMode = PickingMode.Position;
            handle.userData = direction;
            handle.AddToClassList("resize-handle");
            handle.AddToClassList(className);

            Add(handle);
            resizeHandles.Add(handle);
            ConfigureResizeHandleLayout(handle, direction);
            handle.BringToFront();
            handle.RegisterCallback<MouseDownEvent>(OnResizeHandleMouseDown);
            handle.RegisterCallback<MouseMoveEvent>(OnResizeHandleMouseMove);
            handle.RegisterCallback<MouseUpEvent>(OnResizeHandleMouseUp);
            return handle;
        }

        private void OnResizeHandleMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            VisualElement handle = evt.currentTarget as VisualElement;
            if (handle == null || !(handle.userData is ResizeDirection direction))
            {
                return;
            }

            BeginResize(direction, evt.mousePosition);
            resizeCaptureTarget = handle;
            MouseCaptureController.CaptureMouse(resizeCaptureTarget);
            evt.StopImmediatePropagation();
        }

        private void OnResizeHandleMouseMove(MouseMoveEvent evt)
        {
            if (!isResizing || resizeCaptureTarget == null || !MouseCaptureController.HasMouseCapture(resizeCaptureTarget))
            {
                return;
            }

            UpdateResize(evt.mousePosition);
            evt.StopImmediatePropagation();
        }

        private void OnResizeHandleMouseUp(MouseUpEvent evt)
        {
            if (!isResizing || evt.button != 0)
            {
                return;
            }

            isResizing = false;
            if (resizeCaptureTarget != null && MouseCaptureController.HasMouseCapture(resizeCaptureTarget))
            {
                MouseCaptureController.ReleaseMouse(resizeCaptureTarget);
            }

            resizeCaptureTarget = null;
            RestoreMovableCapability();
            evt.StopImmediatePropagation();

            style.borderTopWidth = StyleKeyword.Null;
            style.borderBottomWidth = StyleKeyword.Null;
            style.borderLeftWidth = StyleKeyword.Null;
            style.borderRightWidth = StyleKeyword.Null;
            style.borderTopColor = StyleKeyword.Null;
            style.borderBottomColor = StyleKeyword.Null;
            style.borderLeftColor = StyleKeyword.Null;
            style.borderRightColor = StyleKeyword.Null;

            OnSizeChanged();
            nodeGraphView?.MarkDirty();
        }

        private void OnNodeResizeMouseDown(MouseDownEvent evt)
        {
            if (isResizing)
            {
                evt.StopImmediatePropagation();
                return;
            }

            if (evt.button != 0 || IsPortEventTarget(evt.target))
            {
                return;
            }

            ResizeDirection direction = GetResizeDirection(evt.localMousePosition);
            if (direction == ResizeDirection.None)
            {
                return;
            }

            BeginResize(direction, evt.mousePosition);
            resizeCaptureTarget = this;
            MouseCaptureController.CaptureMouse(resizeCaptureTarget);
            evt.StopImmediatePropagation();
        }

        private void OnNodeResizeMouseMove(MouseMoveEvent evt)
        {
            if (!isResizing)
            {
                UpdateResizeHover(evt.localMousePosition, evt.target);
                return;
            }

            if (resizeCaptureTarget == null || !MouseCaptureController.HasMouseCapture(resizeCaptureTarget))
            {
                return;
            }

            UpdateResize(evt.mousePosition);
            evt.StopImmediatePropagation();
        }

        private void OnNodeResizeMouseUp(MouseUpEvent evt)
        {
            if (!isResizing || evt.button != 0)
            {
                return;
            }

            isResizing = false;
            if (resizeCaptureTarget != null && MouseCaptureController.HasMouseCapture(resizeCaptureTarget))
            {
                MouseCaptureController.ReleaseMouse(resizeCaptureTarget);
            }

            resizeCaptureTarget = null;
            RestoreMovableCapability();
            ClearResizeHighlight();
            OnSizeChanged();
            nodeGraphView?.MarkDirty();
            evt.StopImmediatePropagation();
        }

        protected virtual void OnSizeChanged()
        {
        }

        public virtual void SetDebugHighlight(bool active)
        {
            if (active)
            {
                AddToClassList("ng-node-debug");
            }
            else
            {
                RemoveFromClassList("ng-node-debug");
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            if (newPos.width > 0)
            {
                style.width = newPos.width;
            }

            if (newPos.height > 0)
            {
                style.height = newPos.height;
            }

            UpdateResizeHandlePosition();
        }

        protected void UpdateResizeHandlePosition()
        {
            foreach (VisualElement handle in resizeHandles)
            {
                if (handle.userData is ResizeDirection direction)
                {
                    ConfigureResizeHandleLayout(handle, direction);
                }
            }
        }

        private void ConfigureResizeHandleLayout(VisualElement handle, ResizeDirection direction)
        {
            const float EdgeSize = 8;
            const float CornerSize = 14;
            const float OutsideOffset = -4;

            handle.style.left = StyleKeyword.Null;
            handle.style.right = StyleKeyword.Null;
            handle.style.top = StyleKeyword.Null;
            handle.style.bottom = StyleKeyword.Null;
            handle.style.width = StyleKeyword.Null;
            handle.style.height = StyleKeyword.Null;

            bool left = direction.HasFlag(ResizeDirection.Left);
            bool right = direction.HasFlag(ResizeDirection.Right);
            bool top = direction.HasFlag(ResizeDirection.Top);
            bool bottom = direction.HasFlag(ResizeDirection.Bottom);
            bool corner = (left || right) && (top || bottom);

            if (corner)
            {
                handle.style.width = CornerSize;
                handle.style.height = CornerSize;
            }
            else if (left || right)
            {
                handle.style.width = EdgeSize;
                handle.style.top = CornerSize;
                handle.style.bottom = CornerSize;
            }
            else
            {
                handle.style.height = EdgeSize;
                handle.style.left = CornerSize;
                handle.style.right = CornerSize;
            }

            if (left)
            {
                handle.style.left = OutsideOffset;
            }
            else if (right)
            {
                handle.style.right = OutsideOffset;
            }

            if (top)
            {
                handle.style.top = OutsideOffset;
            }
            else if (bottom)
            {
                handle.style.bottom = OutsideOffset;
            }
        }

        private ResizeDirection GetResizeDirection(Vector2 localMousePosition)
        {
            Vector2 size = GetCurrentSize(GetPosition());
            bool left = localMousePosition.x <= resizeEdgeSize;
            bool right = localMousePosition.x >= size.x - resizeEdgeSize;
            bool top = localMousePosition.y <= resizeEdgeSize;
            bool bottom = localMousePosition.y >= size.y - resizeEdgeSize;

            ResizeDirection direction = ResizeDirection.None;
            if (left)
            {
                direction |= ResizeDirection.Left;
            }
            else if (right)
            {
                direction |= ResizeDirection.Right;
            }

            if (top)
            {
                direction |= ResizeDirection.Top;
            }
            else if (bottom)
            {
                direction |= ResizeDirection.Bottom;
            }

            return direction;
        }

        private void BeginResize(ResizeDirection direction, Vector2 mousePosition)
        {
            isResizing = true;
            resizeDirection = direction;
            resizeStartPosition = mousePosition;
            Rect currentRect = GetPosition();
            resizeStartRect = new Rect(currentRect.position, GetCurrentSize(currentRect));
            DisableMovableForResize();
        }

        private void UpdateResize(Vector2 mousePosition)
        {
            Vector2 delta = GetGraphDelta(mousePosition - resizeStartPosition);
            Rect newRect = CalculateResizeRect(delta);
            SetPosition(newRect);
            RefreshPorts();
        }

        private void RestoreMovableCapability()
        {
            if (resizeStartMovable || resizeHoverDisabledMovable)
            {
                capabilities |= Capabilities.Movable;
            }

            resizeStartMovable = false;
            resizeHoverDisabledMovable = false;
        }

        private void DisableMovableForResize()
        {
            if ((capabilities & Capabilities.Movable) == Capabilities.Movable)
            {
                resizeStartMovable = true;
                capabilities &= ~Capabilities.Movable;
            }
        }

        private void UpdateResizeHover(Vector2 localMousePosition, IEventHandler target)
        {
            if (IsPortEventTarget(target))
            {
                if (resizeHoverDisabledMovable)
                {
                    capabilities |= Capabilities.Movable;
                    resizeHoverDisabledMovable = false;
                }

                return;
            }

            ResizeDirection direction = GetResizeDirection(localMousePosition);
            if (direction != ResizeDirection.None)
            {
                if ((capabilities & Capabilities.Movable) == Capabilities.Movable)
                {
                    resizeHoverDisabledMovable = true;
                    capabilities &= ~Capabilities.Movable;
                }
            }
            else if (resizeHoverDisabledMovable)
            {
                capabilities |= Capabilities.Movable;
                resizeHoverDisabledMovable = false;
            }
        }

        private bool IsPortEventTarget(IEventHandler target)
        {
            VisualElement element = target as VisualElement;
            while (element != null)
            {
                if (element is Port)
                {
                    return true;
                }

                element = element.parent;
            }

            return false;
        }

        private void OnNodeResizeMouseLeave(MouseLeaveEvent evt)
        {
            if (!isResizing && resizeHoverDisabledMovable)
            {
                capabilities |= Capabilities.Movable;
                resizeHoverDisabledMovable = false;
            }
        }

        private void ClearResizeHighlight()
        {
            style.borderTopWidth = StyleKeyword.Null;
            style.borderBottomWidth = StyleKeyword.Null;
            style.borderLeftWidth = StyleKeyword.Null;
            style.borderRightWidth = StyleKeyword.Null;
            style.borderTopColor = StyleKeyword.Null;
            style.borderBottomColor = StyleKeyword.Null;
            style.borderLeftColor = StyleKeyword.Null;
            style.borderRightColor = StyleKeyword.Null;
        }

        private Rect CalculateResizeRect(Vector2 delta)
        {
            float xMin = resizeStartRect.xMin;
            float xMax = resizeStartRect.xMax;
            float yMin = resizeStartRect.yMin;
            float yMax = resizeStartRect.yMax;

            if (resizeDirection.HasFlag(ResizeDirection.Left))
            {
                xMin = Mathf.Clamp(xMin + delta.x, xMax - maxSize.x, xMax - minSize.x);
            }
            else if (resizeDirection.HasFlag(ResizeDirection.Right))
            {
                xMax = Mathf.Clamp(xMax + delta.x, xMin + minSize.x, xMin + maxSize.x);
            }

            if (resizeDirection.HasFlag(ResizeDirection.Top))
            {
                yMin = Mathf.Clamp(yMin + delta.y, yMax - maxSize.y, yMax - minSize.y);
            }
            else if (resizeDirection.HasFlag(ResizeDirection.Bottom))
            {
                yMax = Mathf.Clamp(yMax + delta.y, yMin + minSize.y, yMin + maxSize.y);
            }

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        private Vector2 GetGraphDelta(Vector2 panelDelta)
        {
            if (nodeGraphView == null)
            {
                return panelDelta;
            }

            Vector3 scale = nodeGraphView.viewTransform.scale;
            float scaleX = Mathf.Approximately(scale.x, 0) ? 1 : scale.x;
            float scaleY = Mathf.Approximately(scale.y, 0) ? 1 : scale.y;
            return new Vector2(panelDelta.x / scaleX, panelDelta.y / scaleY);
        }

        private Vector2 GetCurrentSize(Rect rect)
        {
            float width = GetValidSize(rect.width, resolvedStyle.width, layout.width, minSize.x);
            float height = GetValidSize(rect.height, resolvedStyle.height, layout.height, minSize.y);
            return new Vector2(
                Mathf.Clamp(width, minSize.x, maxSize.x),
                Mathf.Clamp(height, minSize.y, maxSize.y));
        }

        private float GetValidSize(float preferred, float resolved, float layoutValue, float fallback)
        {
            if (IsValidSize(preferred))
            {
                return preferred;
            }

            if (IsValidSize(resolved))
            {
                return resolved;
            }

            if (IsValidSize(layoutValue))
            {
                return layoutValue;
            }

            return fallback;
        }

        private bool IsValidSize(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0;
        }

        [Flags]
        private enum ResizeDirection
        {
            None = 0,
            Left = 1,
            Right = 2,
            Top = 4,
            Bottom = 8
        }

        protected void ChangeColor(StyleColor color)
        {
            titleContainer.style.backgroundColor = color;
        }

        protected void ResetColor()
        {
            titleContainer.style.backgroundColor = m_originColor;
        }
    }
}
