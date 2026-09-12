using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NodeGraphView.Editor
{
    public class GraphEditorUnility
    {
        private const string DEFAULT_FILE_SAVE_NAME = "NewGraph{0}";
        private const string DEFAULT_FILE_PATH = "Assets/NodeGraphView/Editor Default Resources/GraphData";
        private const string BYTES_FILE_PATH = "Assets/Resources";

        public static NodeGraphNodeData CreateDefaultNodeData(bool isStartNode)
        {
            Type nodeType = isStartNode ? typeof(StartNode) : typeof(NodeBase);
            return CreateDefaultNodeData(nodeType, isStartNode);
        }

        public static NodeGraphNodeData CreateDefaultNodeData(Type nodeViewType, bool isStartNode)
        {
            NodeGraphController controller = new NodeGraphController();
            return controller.CreateNodeData(null, nodeViewType, isStartNode);
        }

        public static void CreateDefaultGraph()
        {
            CreateGraph<NodeGraphAsset>();
        }

        public static TAsset CreateGraph<TAsset>() where TAsset : NodeGraphAsset
        {
            return (TAsset)CreateGraph(typeof(TAsset));
        }

        public static NodeGraphAsset CreateGraph(Type assetType)
        {
            EnsureAssetFolder(DEFAULT_FILE_PATH);

            NodeGraphAsset asset = ScriptableObject.CreateInstance(assetType) as NodeGraphAsset;
            if (asset == null)
            {
                Debug.LogError($"{assetType.FullName} must inherit NodeGraphAsset.");
                return null;
            }

            int soFileCount = CountFilesExcludingMeta(DEFAULT_FILE_PATH);
            asset.graphName = string.Format(DEFAULT_FILE_SAVE_NAME, soFileCount);
            string defaultFileName = asset.graphName + ".asset";
            string defaultFilePath = Path.Combine(DEFAULT_FILE_PATH, defaultFileName);

            NodeGraphController controller = NodeGraphControllerRegistry.GetController(asset);
            NodeGraphDocument document = new NodeGraphDocument();
            document.graphId = asset.GraphId;
            document.graphType = asset.GetType().FullName;
            NodeGraphNodeData defaultNode = controller.CreateNodeData(asset, typeof(StartNode), true);
            document.nodes.Add(defaultNode);
            asset.SetSerializedGraph(controller.SerializeGraph(asset, document));

            AssetDatabase.CreateAsset(asset, defaultFilePath);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return asset;
        }

        public static void SaveDataToBytes(string data, string fileName)
        {
            SaveDataToFile(data, fileName, BYTES_FILE_PATH, ".bytes");
        }

        public static void SaveDataToFile(string data, string fileName, string folderPath, string extension)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (string.IsNullOrEmpty(extension))
            {
                extension = ".bytes";
            }

            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            string safeFileName = string.IsNullOrEmpty(fileName) ? "NodeGraph" : fileName;
            string fileFullPath = Path.Combine(folderPath, safeFileName + extension);
            File.WriteAllText(fileFullPath, data);
            EditorUtility.DisplayDialog("Save Success", $"Save Path:{fileFullPath}", "OK");
            AssetDatabase.Refresh();
        }

        public static IEnumerable<Type> GetNodeTypesForAsset(Type assetType)
        {
            List<Type> result = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    result.AddRange(assembly.GetTypes()
                        .Where(t =>
                            typeof(NodeBase).IsAssignableFrom(t) &&
                            !t.IsAbstract &&
                            !t.IsGenericType &&
                            IsNodeAvailableForAsset(t, assetType)));
                }
                catch (ReflectionTypeLoadException)
                {
                }
            }

            return result.OrderBy(t => t.FullName);
        }

        public static int CountFilesExcludingMeta(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                return 0;
            }

            string[] allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
            int count = 0;

            foreach (string file in allFiles)
            {
                if (!file.EndsWith(".meta"))
                {
                    count++;
                }
            }

            return count;
        }

        private static void EnsureAssetFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static bool IsNodeAvailableForAsset(Type nodeType, Type assetType)
        {
            NodeGraphNodeAttribute nodeAttribute = GetNodeAttribute(nodeType);
            if (nodeAttribute != null)
            {
                return nodeAttribute.AssetType == assetType || assetType.IsSubclassOf(nodeAttribute.AssetType);
            }

            return assetType == typeof(NodeGraphAsset);
        }

        public static NodeGraphNodeAttribute GetNodeAttribute(Type nodeType)
        {
            return nodeType.GetCustomAttribute<NodeGraphNodeAttribute>();
        }
    }
}
