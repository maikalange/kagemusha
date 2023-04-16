using Newtonsoft.Json;

namespace LegalMindPersistence
{
    internal class JsonSection
    {
        [JsonProperty(PropertyName = "sectionNo")]
        public string sectionNo;
        [JsonProperty(PropertyName = "actName")]
        public string actName;
        [JsonProperty(PropertyName = "sectionTitle")]
        public string sectionTitle;
        [JsonProperty(PropertyName = "sectionContent")]
        public string sectionContent;
        
        public JsonSection(string sectionNo, string actName, string sectionTitle, string sectionContent)
        {
            this.sectionNo = sectionNo;
            this.actName = actName;
            this.sectionTitle = sectionTitle;
            this.sectionContent = sectionContent;
        }
    }
}