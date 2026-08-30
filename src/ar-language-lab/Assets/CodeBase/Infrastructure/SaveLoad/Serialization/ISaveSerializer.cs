namespace CodeBase.Infrastructure.SaveLoad.Serialization
{
    public interface ISaveSerializer
    {
        string Serialize<TSaveData>(TSaveData data) where TSaveData : class;
        TSaveData Deserialize<TSaveData>(string payload) where TSaveData : class;
    }
}


