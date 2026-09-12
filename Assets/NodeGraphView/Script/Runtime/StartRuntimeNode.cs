namespace NodeGraphView
{
    public class StartRuntimeNode : NodeGraphRuntimeNodeBase
    {
        protected override void DeserializeUserData(string userDataJson)
        {
        }

        public override string Update()
        {
            return "Out";
        }
    }
}
