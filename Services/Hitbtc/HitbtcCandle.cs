using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Hitbtc
{
    public class HitbtcCandle
    {
        public DateTime timestamp { get; set; }
        public string open { get; set; }
        public string close { get; set; }
        public string min { get; set; }
        public string max { get; set; }
        public string volume { get; set; }
        public string volumeQuote { get; set; }
    }
}
