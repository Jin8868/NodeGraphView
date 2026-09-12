using System;
using System.Collections.Generic;

namespace NodeGraphView
{
    public static class GraphDebugService
    {
        private static readonly Dictionary<string, long> CurrentNodeIds = new Dictionary<string, long>();
        private static readonly Dictionary<string, GraphRunner> Runners = new Dictionary<string, GraphRunner>();

        public static event Action<string, long> CurrentNodeChanged;

        public static void RegisterRunner(GraphRunner runner)
        {
            if (runner == null || string.IsNullOrEmpty(runner.GraphId))
            {
                return;
            }

            Runners[runner.GraphId] = runner;
        }

        public static void UnregisterRunner(GraphRunner runner)
        {
            if (runner == null || string.IsNullOrEmpty(runner.GraphId))
            {
                return;
            }

            Runners.Remove(runner.GraphId);
            Clear(runner.GraphId);
        }

        public static bool Step(string graphId)
        {
            if (string.IsNullOrEmpty(graphId) || !Runners.TryGetValue(graphId, out GraphRunner runner))
            {
                return false;
            }

            runner.Step();
            NotifyCurrentNodeChanged(graphId, runner.CurrentNode == null ? 0 : runner.CurrentNode.NodeId);
            return true;
        }

        public static bool TryGetCurrentNodeId(string graphId, out long nodeId)
        {
            return CurrentNodeIds.TryGetValue(graphId, out nodeId);
        }

        public static void NotifyCurrentNodeChanged(string graphId, long nodeId)
        {
            if (string.IsNullOrEmpty(graphId))
            {
                return;
            }

            CurrentNodeIds[graphId] = nodeId;
            CurrentNodeChanged?.Invoke(graphId, nodeId);
        }

        public static void Clear(string graphId)
        {
            if (string.IsNullOrEmpty(graphId))
            {
                return;
            }

            CurrentNodeIds.Remove(graphId);
            CurrentNodeChanged?.Invoke(graphId, 0);
        }
    }
}
