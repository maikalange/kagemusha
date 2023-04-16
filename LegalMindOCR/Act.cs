using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegalMindOCR
{
    public class Act
    {
        [JsonProperty("pages")]
        public Page[] Pages { get; set; }
        public List<Definition> Definitions
        {
            get;set;
        }
        [JsonProperty("sections")]
        public List<Section> Sections { get; set; }
    }
}
