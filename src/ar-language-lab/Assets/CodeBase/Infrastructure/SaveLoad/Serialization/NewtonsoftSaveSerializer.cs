using Newtonsoft.Json;

namespace CodeBase.Infrastructure.SaveLoad.Serialization
{
    public class NewtonsoftSaveSerializer : ISaveSerializer
    {
        private readonly JsonSerializerSettings _settings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
        };

        public string Serialize<TSaveData>(TSaveData data) where TSaveData : class
        {
            return JsonConvert.SerializeObject(data, _settings);
        }

        public TSaveData Deserialize<TSaveData>(string payload) where TSaveData : class
        {
            if (string.IsNullOrWhiteSpace(payload))
                return null;

            return JsonConvert.DeserializeObject<TSaveData>(payload, _settings);
        }
    }
}


