using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Hitbtc
{
    public class HitbtcTicker
    {
        public string ask { get; set; }
        public string bid { get; set; }
        public string last { get; set; }
        public string open { get; set; }
        public string low { get; set; }
        public string high { get; set; }
        public string volume { get; set; }
        public string volumeQuote { get; set; }
        public DateTime timestamp { get; set; }
        public string symbol { get; set; }
    }
}
