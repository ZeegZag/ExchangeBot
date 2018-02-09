using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Gdax
{
    public class GdaxOrders
    {
        public long sequence { get; set; }
        public List<List<object>> bids { get; set; }
        public List<List<object>> asks { get; set; }
    }
}
