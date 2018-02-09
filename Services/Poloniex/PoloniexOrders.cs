using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Poloniex
{
    public class PoloniexOrders
    {
        public List<List<object>> asks { get; set; }
        public List<List<object>> bids { get; set; }
        public string isFrozen { get; set; }
        public long seq { get; set; }
    }
}
