namespace NodeGraphView
{
    public abstract class NodeGraphRuntimeNodeBase<TUserData> : NodeGraphRuntimeNodeBase where TUserData : new()
    {
        protected TUserData customData;

        public TUserData UserData => customData;

        protected override void DeserializeUserData(string userDataJson)
        {
            if (string.IsNullOrEmpty(userDataJson))
            {
                customData = new TUserData();
                return;
            }

            TUserData data = GraphRuntimeManager.Instance.Deserialize<TUserData>(userDataJson);
            customData = data == null ? new TUserData() : data;
        }
    }
}
