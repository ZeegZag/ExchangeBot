using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ZeegZag.Crawler2.Services
{
    public class RequestModel
    {
        [JsonProperty("id")]
        public string RequestId { get; set; }
        [JsonProperty("head")]
        public List<Tuple<string, string>> Headers { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("res")]
        public string Response { get; set; }
        [JsonProperty("ok")]
        public bool IsSuccess { get; set; }
    }
}
