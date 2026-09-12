using UnityEngine;

namespace NodeGraphView
{
    [CreateAssetMenu(fileName = "NodeGraph", menuName = "Node Graph/Default Graph")]
    public class NodeGraphAsset : ScriptableObject
    {
        [SerializeField] private string graphId;
        [SerializeField] public string graphName;

        [SerializeField, TextArea(5, 20)] private string serializedGraph;

        public string GraphId
        {
            get
            {
                if (string.IsNullOrEmpty(graphId))
                {
                    graphId = GraphUtility.GenerateID().ToString();
                }

                return graphId;
            }
        }

        public string SerializedGraph => serializedGraph;

        public void SetSerializedGraph(string value)
        {
            serializedGraph = value;
        }
    }
}
