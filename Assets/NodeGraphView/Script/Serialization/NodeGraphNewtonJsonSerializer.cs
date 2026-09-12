using Newtonsoft.Json;

namespace NodeGraphView
{
    public interface IGraphSerializer
    {
        string Serialize<T>(T data);
    }

    public interface IGraphDeserializer
    {
        T Deserialize<T>(string serializedData) where T : new();
    }

    public interface INodeGraphSerializer : IGraphSerializer, IGraphDeserializer
    {
    }

    public abstract class NodeGraphSerializer : INodeGraphSerializer
    {
        public abstract string Serialize<T>(T data);
        public abstract T Deserialize<T>(string serializedData) where T : new();
    }

    public sealed class NodeGraphNewtonJsonSerializer : NodeGraphSerializer
    {
        public override string Serialize<T>(T data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }

        public override T Deserialize<T>(string serializedData)
        {
            if (string.IsNullOrEmpty(serializedData))
            {
                return new T();
            }

            return JsonConvert.DeserializeObject<T>(serializedData) ?? new T();
        }
    }
}
