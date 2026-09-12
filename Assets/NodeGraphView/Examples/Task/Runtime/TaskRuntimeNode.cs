using NodeGraphView;
using UnityEngine;
using Game.NodeGraphs.View;

namespace Game.NodeGraphs
{
    [NodeRuntime("Game.NodeGraphs.Editor.TaskNode")]
    public class TaskRuntimeNode : NodeGraphRuntimeNodeBase<TaskNodeData>, ITaskPanelCallbacks
    {
        private TaskPanel panel;

        public void SetPanel(TaskPanel taskPanel)
        {
            panel = taskPanel;
        }

        public override void OnEnter()
        {
            Debug.Log($"Enter task: {UserData.name} ({NodeId})");
            panel?.SetCallbacks(this);
            panel?.Refresh(
                string.IsNullOrEmpty(UserData.name) ? Title : UserData.name,
                UserData.description,
                HasExit("complete"),
                HasExit("failed"));
        }

        public void SelectExit(string exitKey)
        {
            if (runner == null || !runner.MoveNext(exitKey))
            {
                return;
            }

            AdvanceToTaskNode(runner);
        }

        public void OnComplete()
        {
            SelectExit("complete");
        }

        public void OnFail()
        {
            SelectExit("failed");
        }

        private bool HasExit(string exitKey)
        {
            return NodeData != null &&
                   NodeData.nextNodeMap != null &&
                   NodeData.nextNodeMap.ContainsKey(exitKey);
        }

        public static void AdvanceToTaskNode(GraphRunner graphRunner)
        {
            while (graphRunner != null &&
                   graphRunner.IsRunning &&
                   graphRunner.CurrentNode != null &&
                   !(graphRunner.CurrentNode is TaskRuntimeNode))
            {
                graphRunner.Step();
            }
        }

        public override string Update()
        {
            return null;
        }

        public override void OnLeave()
        {
            Debug.Log($"Leave task: {UserData.name} ({NodeId})");
            panel?.Hide();
        }
    }
}
