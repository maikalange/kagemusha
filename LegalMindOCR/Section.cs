using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace LegalMindOCR
{
    public class Section
    {
        [JsonProperty(PropertyName = "sectionTitle")]
        public string SectionTitle { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public enum LocationOfSection { TOC=1,BODY}
        [JsonProperty(PropertyName ="sectionNo")]
        public string Id { get; private set; }
        [JsonProperty("sectionLocation")]
        public LocationOfSection SecLocation { get; private set; }
        [JsonProperty(PropertyName = "sectionContent")]
        public string Content { get; set; }
        public Section(string id, string content, LocationOfSection secLocation)
        {
            Id = id;
            Content = (content);
            SecLocation = secLocation;
        }

    }
}
