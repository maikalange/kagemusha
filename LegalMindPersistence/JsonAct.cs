using Newtonsoft.Json;

namespace LegalMindPersistence
{
    internal class JsonAct
    {
        [JsonProperty(PropertyName = "actName")]
        public string actName;
        [JsonProperty(PropertyName = "content")]
        public string content;

        public JsonAct(string actName, string content)
        {
            this.actName = actName;
            this.content = content;
        }
    }
}