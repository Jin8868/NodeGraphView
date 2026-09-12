namespace NodeGraphView
{
    public class NodeBase<TUserData> : NodeBase where TUserData : new()
    {
        protected TUserData customData;

        public TUserData UserData => customData;

        protected override void InitData()
        {
            base.InitData();
            customData = DeserializeUserData();
        }

        protected override string SerializeUserData()
        {
            return GraphRuntimeManager.Instance.Serialize(customData);
        }

        public override object GetUserDataObject()
        {
            return customData;
        }

        protected virtual TUserData DeserializeUserData()
        {
            if (string.IsNullOrEmpty(nodeData.userDataJson))
            {
                return new TUserData();
            }

            TUserData data = GraphRuntimeManager.Instance.Deserialize<TUserData>(nodeData.userDataJson);
            return data == null ? new TUserData() : data;
        }
    }
}
