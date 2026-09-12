using UnityEngine;

namespace NodeGraphView
{
    public struct NodeGraphVector2
    {
        public float x;
        public float y;

        public NodeGraphVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Vector2(NodeGraphVector2 value)
        {
            return new Vector2(value.x, value.y);
        }

        public static implicit operator NodeGraphVector2(Vector2 value)
        {
            return new NodeGraphVector2(value.x, value.y);
        }
    }
}
