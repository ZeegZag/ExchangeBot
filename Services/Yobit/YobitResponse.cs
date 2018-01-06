using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ZeegZag.Crawler2.Services.Yobit
{
    public class YobitResponse
    {
        public int server_time { get; set; }
        public JToken pairs { get; set; }
    }
}
