using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Gdax
{
    public class GdaxTicker
    {
        public string open { get; set; }
        public string high { get; set; }
        public string low { get; set; }
        public string volume { get; set; }
        public string last { get; set; }
        public string volume_30day { get; set; }
    }
}
