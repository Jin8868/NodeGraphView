using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeGraphView.Editor
{
    public class NodeGraphScaffoldWindow : EditorWindow
    {
        private TextField graphNameField;
        private TextField namespaceField;
        private ObjectField outputFolderField;
        private Toggle createNodeToggle;
        private VisualElement nodeOptionsContainer;
        private TextField nodeClassField;
        private TextField nodeViewClassField;
        private TextField runtimeNodeClassField;
        private Toggle createNodeViewToggle;
        private Toggle createRuntimeNodeToggle;
        private Toggle createDebuggerToggle;

        [MenuItem("Tools/Node Graph/Create Graph Scripts")]
        public static void Open()
        {
            GetWindow<NodeGraphScaffoldWindow>("Node Graph Creator");
        }

        public void CreateGUI()
        {
            rootVisualElement.style.paddingLeft = 8;
            rootVisualElement.style.paddingRight = 8;
            rootVisualElement.style.paddingTop = 8;
            rootVisualElement.style.paddingBottom = 8;

            graphNameField = new TextField("Graph Name") { value = string.Empty };
            namespaceField = new TextField("Namespace") { value = "Game.NodeGraphs" };
            VisualElement outputFolderRow = new VisualElement();
            outputFolderRow.style.flexDirection = FlexDirection.Row;

            outputFolderField = new ObjectField("Output Folder")
            {
                objectType = typeof(DefaultAsset),
                value = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets")
            };
            outputFolderField.style.flexGrow = 1;
            Button browseButton = new Button(BrowseOutputFolder) { text = "..." };
            browseButton.style.width = 32;

            outputFolderRow.Add(outputFolderField);
            outputFolderRow.Add(browseButton);

            createNodeToggle = new Toggle("Create Node") { value = true };
            createNodeViewToggle = new Toggle("Create Node View") { value = true };
            createRuntimeNodeToggle = new Toggle("Create Runtime Node") { value = true };
            createDebuggerToggle = new Toggle("Create Debugger") { value = true };

            nodeOptionsContainer = new VisualElement();
            nodeOptionsContainer.style.marginLeft = 16;
            nodeClassField = CreateReadOnlyTextField("Node Class");
            nodeViewClassField = CreateReadOnlyTextField("Node View Class");
            runtimeNodeClassField = CreateReadOnlyTextField("Runtime Node Class");

            nodeOptionsContainer.Add(nodeClassField);
            nodeOptionsContainer.Add(createNodeViewToggle);
            nodeOptionsContainer.Add(nodeViewClassField);
            nodeOptionsContainer.Add(createRuntimeNodeToggle);
            nodeOptionsContainer.Add(runtimeNodeClassField);

            Button createButton = new Button(CreateScripts) { text = "Create" };

            graphNameField.RegisterValueChangedCallback(_ => RefreshNodeOptions());
            outputFolderField.RegisterValueChangedCallback(evt => ValidateOutputFolder(evt.previousValue as DefaultAsset));
            createNodeToggle.RegisterValueChangedCallback(_ => RefreshNodeOptions());
            createNodeViewToggle.RegisterValueChangedCallback(_ => RefreshNodeOptions());
            createRuntimeNodeToggle.RegisterValueChangedCallback(_ => RefreshNodeOptions());

            rootVisualElement.Add(graphNameField);
            rootVisualElement.Add(namespaceField);
            rootVisualElement.Add(outputFolderRow);
            rootVisualElement.Add(createNodeToggle);
            rootVisualElement.Add(nodeOptionsContainer);
            rootVisualElement.Add(createDebuggerToggle);
            rootVisualElement.Add(createButton);

            RefreshNodeOptions();
        }

        private void CreateScripts()
        {
            string graphName = ToPascalName(graphNameField.value, "NewGraph");
            string nodeName = graphName;
            string ns = string.IsNullOrEmpty(namespaceField.value) ? "Game.NodeGraphs" : namespaceField.value.Trim();
            string folder = GetSelectedFolderPath();
            string editorFolder = Path.Combine(folder, "Editor").Replace("\\", "/");
            string runtimeFolder = Path.Combine(folder, "Runtime").Replace("\\", "/");

            Directory.CreateDirectory(folder);
            Directory.CreateDirectory(editorFolder);
            Directory.CreateDirectory(runtimeFolder);

            WriteFile(Path.Combine(folder, $"{graphName}GraphAsset.cs"), CreateGraphAssetCode(ns, graphName));
            WriteFile(Path.Combine(editorFolder, $"{graphName}GraphController.cs"), CreateGraphControllerCode(ns, graphName));
            WriteFile(Path.Combine(editorFolder, $"{graphName}GraphWindow.cs"), CreateGraphWindowCode(ns, graphName, nodeName, createNodeToggle.value));

            if (createNodeToggle.value)
            {
                WriteFile(Path.Combine(folder, $"{nodeName}NodeData.cs"), CreateNodeDataCode(ns, nodeName));
                WriteFile(Path.Combine(editorFolder, $"{nodeName}Node.cs"), CreateEditorNodeCode(ns, graphName, nodeName, createNodeViewToggle.value, createRuntimeNodeToggle.value));

                if (createNodeViewToggle.value)
                {
                    WriteFile(Path.Combine(editorFolder, $"{nodeName}NodeView.cs"), CreateNodeViewCode(ns, nodeName));
                }

                if (createRuntimeNodeToggle.value)
                {
                    WriteFile(Path.Combine(runtimeFolder, $"{nodeName}RuntimeNode.cs"), CreateRuntimeNodeCode(ns, nodeName));
                }
            }

            if (createDebuggerToggle.value)
            {
                WriteFile(Path.Combine(editorFolder, $"{graphName}GraphDebugger.cs"), CreateDebuggerCode(ns, graphName));
            }

            AssetDatabase.Refresh();
        }

        private void RefreshNodeOptions()
        {
            bool createNode = createNodeToggle.value;
            nodeOptionsContainer.style.display = createNode ? DisplayStyle.Flex : DisplayStyle.None;

            string graphName = ToPascalName(graphNameField.value, "NewGraph");
            nodeClassField.SetValueWithoutNotify($"{graphName}Node");
            nodeViewClassField.SetValueWithoutNotify($"{graphName}NodeView");
            runtimeNodeClassField.SetValueWithoutNotify($"{graphName}RuntimeNode");
            nodeViewClassField.style.display = createNodeViewToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
            runtimeNodeClassField.style.display = createRuntimeNodeToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static TextField CreateReadOnlyTextField(string label)
        {
            TextField field = new TextField(label);
            field.SetEnabled(false);
            return field;
        }

        private string GetSelectedFolderPath()
        {
            string path = "Assets";
            if (outputFolderField.value != null)
            {
                string selectedPath = AssetDatabase.GetAssetPath(outputFolderField.value);
                if (AssetDatabase.IsValidFolder(selectedPath))
                {
                    path = selectedPath;
                }
            }

            return path;
        }

        private void BrowseOutputFolder()
        {
            string currentPath = GetSelectedFolderPath();
            string absoluteCurrentPath = AssetPathToAbsolutePath(currentPath);
            string absolutePath = EditorUtility.OpenFolderPanel("Output Folder", absoluteCurrentPath, string.Empty);
            if (string.IsNullOrEmpty(absolutePath))
            {
                return;
            }

            string projectPath = Application.dataPath.Replace("\\", "/");
            absolutePath = absolutePath.Replace("\\", "/");
            if (absolutePath == projectPath)
            {
                outputFolderField.value = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets");
                return;
            }

            if (absolutePath.StartsWith(projectPath + "/"))
            {
                string assetPath = "Assets" + absolutePath.Substring(projectPath.Length);
                outputFolderField.value = AssetDatabase.LoadAssetAtPath<DefaultAsset>(assetPath);
                return;
            }

            Debug.LogWarning("Output folder must be inside this Unity project's Assets folder.");
        }

        private void ValidateOutputFolder(DefaultAsset previousValue)
        {
            if (outputFolderField.value == null)
            {
                outputFolderField.SetValueWithoutNotify(previousValue != null
                    ? previousValue
                    : AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets"));
                return;
            }

            string selectedPath = AssetDatabase.GetAssetPath(outputFolderField.value);
            if (AssetDatabase.IsValidFolder(selectedPath))
            {
                return;
            }

            Debug.LogWarning("Output folder must be a folder asset.");
            outputFolderField.SetValueWithoutNotify(previousValue != null
                ? previousValue
                : AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets"));
        }

        private static string AssetPathToAbsolutePath(string assetPath)
        {
            if (assetPath == "Assets")
            {
                return Application.dataPath;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName.Replace("\\", "/");
            return Path.Combine(projectRoot, assetPath).Replace("\\", "/");
        }

        private static void WriteFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        private static string ToPascalName(string value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            string result = string.Empty;
            bool upperNext = true;
            foreach (char c in value.Trim())
            {
                if (!char.IsLetterOrDigit(c))
                {
                    upperNext = true;
                    continue;
                }

                result += upperNext ? char.ToUpperInvariant(c) : c;
                upperNext = false;
            }

            return string.IsNullOrEmpty(result) ? fallback : result;
        }

        private static string CreateGraphAssetCode(string ns, string graphName)
        {
            return
$@"using UnityEngine;

namespace {ns}
{{
    [CreateAssetMenu(fileName = ""{graphName}Graph"", menuName = ""Node Graph/{graphName} Graph"")]
    public class {graphName}GraphAsset : NodeGraphView.NodeGraphAsset
    {{
    }}
}}
";
        }

        private static string CreateGraphControllerCode(string ns, string graphName)
        {
            return
$@"using System;
using NodeGraphView;
using NodeGraphView.Editor;

namespace {ns}.Editor
{{
    public class {graphName}GraphController : NodeGraphController
    {{
        public override Type AssetType => typeof({ns}.{graphName}GraphAsset);
        public override Type EditorWindowType => typeof({graphName}GraphWindow);
        public override Type GraphViewType => typeof(NodeGraphView.NodeGraphView);

        public override string GetExportFolderPath(NodeGraphAsset asset)
        {{
            return ""Assets/Resources/{graphName}Graphs"";
        }}
    }}
}}
";
        }

        private static string CreateGraphWindowCode(string ns, string graphName, string nodeName, bool createNode)
        {
            string toolbar = createNode
                ? $@"
        public override void BuildToolbar(INodeGraphToolbar toolbar)
        {{
            toolbar.AddButton(""{nodeName}"", Create{nodeName}Node);
        }}

        private void Create{nodeName}Node()
        {{
            m_graphView?.CreateNode(typeof({nodeName}Node));
        }}
"
                : @"
        public override void BuildToolbar(INodeGraphToolbar toolbar)
        {
        }
";

            return
$@"using NodeGraphView;
using NodeGraphView.Editor;
using UnityEditor;

namespace {ns}.Editor
{{
    [NodeGraphWindow(""{graphName} Graph"", typeof({ns}.{graphName}GraphAsset), typeof(NodeGraphView.NodeGraphView), typeof(NodeGraphView.StartNode))]
    public class {graphName}GraphWindow : NodeEditorWindowBase
    {{
        [MenuItem(""Tools/Node Graph/{graphName}/Create {graphName} Graph"")]
        public static void CreateGraph()
        {{
            GraphEditorUnility.CreateGraph<{ns}.{graphName}GraphAsset>();
        }}
{toolbar}
    }}
}}
";
        }

        private static string CreateNodeDataCode(string ns, string nodeName)
        {
            return
$@"namespace {ns}
{{
    [System.Serializable]
    public class {nodeName}NodeData
    {{
        public string name = ""{nodeName}"";
        public string description = string.Empty;
    }}
}}
";
        }

        private static string CreateEditorNodeCode(string ns, string graphName, string nodeName, bool createNodeView, bool createRuntimeNode)
        {
            string runtimeType = createRuntimeNode ? $", typeof({ns}.{nodeName}RuntimeNode)" : string.Empty;
            string viewOverride = createNodeView
                ? $@"
        protected override System.Type GetNodeViewType()
        {{
            return typeof({nodeName}NodeView);
        }}
"
                : string.Empty;

            return
$@"using NodeGraphView;
using NodeGraphView.Editor;

namespace {ns}.Editor
{{
    [NodeGraphNode(""{nodeName}"", typeof({ns}.{graphName}GraphAsset){runtimeType})]
    public class {nodeName}Node : NodeBase<{ns}.{nodeName}NodeData>
    {{
        protected override void InitData()
        {{
            base.InitData();
            nodeData.title = ""{nodeName}"";
            EnsurePorts();
        }}
{viewOverride}

        private void EnsurePorts()
        {{
            EnsureInputPort(""In"", ""in"", ""In"");
            EnsureOutputPort(""Complete"", ""complete"", ""Out"");
            EnsureOutputPort(""Failed"", ""failed"");
            RemoveUnexpectedInputPorts(""in"");
            RemoveUnexpectedOutputPorts(""complete"", ""failed"");
        }}
    }}
}}
";
        }

        private static string CreateNodeViewCode(string ns, string nodeName)
        {
            return
$@"using NodeGraphView;
using UnityEngine.UIElements;

namespace {ns}.Editor
{{
    public class {nodeName}NodeView : NodeViewBase<{ns}.{nodeName}NodeData>
    {{
        public override void Build(VisualElement container)
        {{
            TextField nameField = new TextField(""Name"") {{ value = Data.name }};
            TextField descriptionField = new TextField(""Description"") {{ value = Data.description }};
            descriptionField.multiline = true;

            nameField.RegisterValueChangedCallback(evt =>
            {{
                Data.name = evt.newValue;
                typedNode?.nodeGraphView?.MarkDirty();
            }});

            descriptionField.RegisterValueChangedCallback(evt =>
            {{
                Data.description = evt.newValue;
                typedNode?.nodeGraphView?.MarkDirty();
            }});

            container.Add(nameField);
            container.Add(descriptionField);
        }}
    }}
}}
";
        }

        private static string CreateRuntimeNodeCode(string ns, string nodeName)
        {
            string editorNodeType = $"{ns}.Editor.{nodeName}Node";
            return
$@"using NodeGraphView;

namespace {ns}
{{
    [NodeRuntime(""{editorNodeType}"")]
    public class {nodeName}RuntimeNode : NodeGraphRuntimeNodeBase<{nodeName}NodeData>
    {{
        public override string Update()
        {{
            return ""complete"";
        }}
    }}
}}
";
        }

        private static string CreateDebuggerCode(string ns, string graphName)
        {
            return
$@"namespace {ns}.Editor
{{
    public class {graphName}GraphDebugger
    {{
    }}
}}
";
        }
    }
}
