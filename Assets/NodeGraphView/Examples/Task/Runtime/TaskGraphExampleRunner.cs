using NodeGraphView;
using UnityEngine;
using Game.NodeGraphs.View;

namespace Game.NodeGraphs
{
    public class TaskGraphExampleRunner : MonoBehaviour
    {
        [SerializeField] private string resourcesPath = "TaskGraphs/NewGraph0";
        [SerializeField] private bool playOnStart = true;
        private GraphRunner graphRunner;
        private TaskPanel taskPanel;

        public GraphRunner Runner => graphRunner;

        private void Awake()
        {
            taskPanel = new TaskPanel(transform);
            taskPanel.Hide();
        }

        private void Start()
        {
            LoadGraph();

            if (playOnStart)
            {
                Play();
            }
        }

        public void LoadGraph()
        {
            GraphRuntimeManager.Instance.Clear();
            graphRunner = GraphRuntimeManager.Instance.LoadFromResources(resourcesPath);

            if (graphRunner == null)
            {
                taskPanel?.Hide();
                Debug.LogError($"Task graph not found in Resources: {resourcesPath}", this);
                return;
            }

            foreach (INodeGraphRuntimeNode node in graphRunner.Nodes)
            {
                if (node is TaskRuntimeNode taskNode)
                {
                    taskNode.SetPanel(taskPanel);
                }
            }

            Debug.Log($"Loaded task graph: {graphRunner.GraphId}", this);
        }

        public void Play()
        {
            if (graphRunner == null)
            {
                LoadGraph();
            }

            if (graphRunner == null)
            {
                return;
            }

            if (!graphRunner.Start())
            {
                taskPanel?.Hide();
                Debug.LogError("Task graph start node was not found.", this);
            }

        }

        private void OnDestroy()
        {
            taskPanel?.Hide();
            GraphRuntimeManager.Instance.Clear();
            graphRunner = null;
        }

    }
}
