using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeGraphView.Editor
{
    public abstract class NodeEditorWindowBase : EditorWindow
    {
        protected NodeGraphViewBase m_graphView;
        [SerializeField]
        protected NodeGraphAsset m_graphAsset;
        protected NodeGraphController m_controller;
        [SerializeField]
        private string m_windowTitle;

        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line)
        {
            NodeGraphAsset nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as NodeGraphAsset;
            if (nodeGraph == null)
            {
                return false;
            }

            Open(nodeGraph);
            return true;
        }

        public static NodeEditorWindowBase Open(NodeGraphAsset nodeGraph)
        {
            NodeGraphController controller = NodeGraphControllerRegistry.GetController(nodeGraph);
            Type windowType = controller.EditorWindowType ?? typeof(NodeGraphWindow);
            NodeEditorWindowBase window = GetWindow(windowType) as NodeEditorWindowBase;
            if (window == null)
            {
                Debug.LogError($"{windowType.FullName} must inherit NodeEditorWindowBase.");
                return null;
            }

            window.SetGraphData(nodeGraph);
            return window;
        }

        public virtual void OnEnable()
        {
            GraphDebugService.CurrentNodeChanged -= OnDebugCurrentNodeChanged;
            GraphDebugService.CurrentNodeChanged += OnDebugCurrentNodeChanged;
            if (m_graphAsset != null)
            {
                m_controller = NodeGraphControllerRegistry.GetController(m_graphAsset);
                m_windowTitle = m_controller.GetEditorTitle(m_graphAsset);
                Init();
                m_graphView.SetController(m_controller);
                m_graphView.RebuildGraph();
                RefreshDebugHighlight();
            }
            else
            {
                Init();
            }
        }

        public virtual void Init()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            AddToolBar();
            CreateGraphView();
        }

        public virtual void SetGraphData(NodeGraphAsset nodeGraph)
        {
            m_graphAsset = nodeGraph;
            m_controller = NodeGraphControllerRegistry.GetController(nodeGraph);
            m_windowTitle = m_controller.GetEditorTitle(nodeGraph);
            RefreshWindowTitle();

            Init();

            m_graphView.SetController(m_controller);
            m_graphView.RebuildGraph();
            RefreshDebugHighlight();
        }

        protected virtual void CreateGraphView()
        {
            DisposeGraphView();

            Type graphViewType = m_controller != null && m_controller.GraphViewType != null
                ? m_controller.GraphViewType
                : typeof(NodeGraphView);

            m_graphView = Activator.CreateInstance(graphViewType) as NodeGraphViewBase;
            if (m_graphView == null)
            {
                Debug.LogError($"{graphViewType.FullName} must inherit NodeGraphViewBase.");
                m_graphView = new NodeGraphView();
            }

            m_graphView.Init(m_graphAsset ?? CreateInstance<NodeGraphAsset>());
            m_graphView.SetController(m_controller ?? new NodeGraphController());
            m_graphView.DirtyChanged -= OnGraphDirtyChanged;
            m_graphView.DirtyChanged += OnGraphDirtyChanged;
            RefreshUnsavedState();
            m_graphView.style.flexGrow = 1;
            rootVisualElement.Add(m_graphView);
        }

        protected virtual void AddToolBar()
        {
            Toolbar toolbar = new Toolbar();
            NodeGraphToolbar adapter = new NodeGraphToolbar(toolbar);
            adapter.AddButton("Save SO", SaveBtnClick);
            adapter.AddButton("Export .bytes", ExportBtnClick);
            adapter.AddButton("Debug Step", DebugStepBtnClick);
            BuildToolbar(adapter);
            m_controller?.BuildToolbar(m_graphAsset, adapter, this);
            rootVisualElement.Add(toolbar);
        }

        public virtual void BuildToolbar(INodeGraphToolbar toolbar)
        {
        }

        protected virtual void SaveBtnClick()
        {
            SaveCurrentGraph(true);
        }

        protected virtual void ExportBtnClick()
        {
            m_graphView?.ExportData();
        }

        protected virtual void DebugStepBtnClick()
        {
            if (m_graphAsset == null)
            {
                return;
            }

            GraphDebugService.Step(m_graphAsset.GraphId);
        }

        protected virtual void RefreshDebugHighlight()
        {
            if (m_graphAsset == null || m_graphView == null)
            {
                return;
            }

            if (GraphDebugService.TryGetCurrentNodeId(m_graphAsset.GraphId, out long nodeId) && nodeId > 0)
            {
                m_graphView.HighlightNode(nodeId);
            }
            else
            {
                m_graphView.ClearDebugHighlight();
            }
        }

        protected virtual void OnDebugCurrentNodeChanged(string graphId, long nodeId)
        {
            if (m_graphAsset == null || m_graphView == null || m_graphAsset.GraphId != graphId)
            {
                return;
            }

            if (nodeId <= 0)
            {
                m_graphView.ClearDebugHighlight();
                return;
            }

            m_graphView.HighlightNode(nodeId);
        }

        protected virtual void OnGraphDirtyChanged(bool isDirty)
        {
            RefreshUnsavedState();
        }

        protected virtual void SaveCurrentGraph(bool showSuccessDialog)
        {
            if (m_graphView == null)
            {
                return;
            }

            m_graphView.SaveGraph();
            RefreshUnsavedState();

            if (showSuccessDialog)
            {
                EditorUtility.DisplayDialog("Save Success", $"{m_windowTitle} saved.", "OK");
            }
        }

        protected virtual void RefreshUnsavedState()
        {
            bool isDirty = m_graphView != null && m_graphView.IsDirty;
            hasUnsavedChanges = isDirty;
            saveChangesMessage = $"{m_windowTitle} has unsaved changes.";
            RefreshWindowTitle();
        }

        protected virtual void RefreshWindowTitle()
        {
            string displayTitle = string.IsNullOrEmpty(m_windowTitle) ? "Node Graph" : m_windowTitle;
            if (m_graphView != null && m_graphView.IsDirty)
            {
                displayTitle = "*" + displayTitle;
            }

            titleContent = new GUIContent(displayTitle);
        }

        public override void SaveChanges()
        {
            SaveCurrentGraph(false);
            base.SaveChanges();
        }

        public override void DiscardChanges()
        {
            m_graphView?.ClearDirty();
            base.DiscardChanges();
        }

        public virtual void OnDestroy()
        {
            GraphDebugService.CurrentNodeChanged -= OnDebugCurrentNodeChanged;
            DisposeGraphView();
        }

        protected virtual void DisposeGraphView()
        {
            if (m_graphView == null)
            {
                return;
            }

            if (rootVisualElement.Contains(m_graphView))
            {
                rootVisualElement.Remove(m_graphView);
            }

            m_graphView.DirtyChanged -= OnGraphDirtyChanged;
            m_graphView.Dispose();
            m_graphView = null;
            RefreshUnsavedState();
        }
    }
}
