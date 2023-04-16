using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegalMindOCR
{
    public class Page
    {
        [JsonProperty("pageNo")]
        public int PageNo { get;set;}
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
