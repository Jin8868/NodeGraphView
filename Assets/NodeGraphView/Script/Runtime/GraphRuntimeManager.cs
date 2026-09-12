using System.Collections.Generic;
using UnityEngine;

namespace NodeGraphView
{
    public class GraphRuntimeManager
    {
        public static GraphRuntimeManager Instance { get; } = new GraphRuntimeManager();

        private GraphRuntimeManager()
        {
        }

        private readonly Dictionary<string, GraphRunner> runners = new Dictionary<string, GraphRunner>();
        private readonly INodeGraphSerializer serializer = new NodeGraphNewtonJsonSerializer();

        public IReadOnlyDictionary<string, GraphRunner> Runners => runners;

        public GraphRunner Load(string json)
        {
            NodeGraphRuntimeDocument document = Deserialize<NodeGraphRuntimeDocument>(json);
            return Load(document);
        }

        public string Serialize<T>(T data)
        {
            return serializer.Serialize(data);
        }

        public T Deserialize<T>(string serializedData) where T : new()
        {
            return serializer.Deserialize<T>(serializedData);
        }

        public GraphRunner Load(TextAsset textAsset)
        {
            return textAsset == null ? null : Load(textAsset.text);
        }

        public GraphRunner LoadFromResources(string resourcesPath)
        {
            if (string.IsNullOrEmpty(resourcesPath))
            {
                return null;
            }

            TextAsset textAsset = Resources.Load<TextAsset>(resourcesPath);
            return Load(textAsset);
        }

        public GraphRunner Load(NodeGraphRuntimeDocument document)
        {
            GraphRunner runner = new GraphRunner();
            runner.Init(document);

            string graphId = string.IsNullOrEmpty(runner.GraphId)
                ? GraphUtility.GenerateID().ToString()
                : runner.GraphId;
            runners[graphId] = runner;
            return runner;
        }

        public bool TryGetRunner(string graphId, out GraphRunner runner)
        {
            return runners.TryGetValue(graphId, out runner);
        }

        public void Update()
        {
            foreach (GraphRunner runner in runners.Values)
            {
                runner.Update();
            }
        }

        public void Unload(string graphId)
        {
            if (!runners.TryGetValue(graphId, out GraphRunner runner))
            {
                return;
            }

            runner.Dispose();
            runners.Remove(graphId);
        }

        public void Clear()
        {
            foreach (GraphRunner runner in runners.Values)
            {
                runner.Dispose();
            }

            runners.Clear();
        }
    }
}
