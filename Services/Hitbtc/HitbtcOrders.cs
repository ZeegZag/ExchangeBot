using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Hitbtc
{
    public class Ask
    {
        public string price { get; set; }
        public string size { get; set; }
    }

    public class Bid
    {
        public string price { get; set; }
        public string size { get; set; }
    }

    public class HitbtcOrders
    {
        public List<Ask> ask { get; set; }
        public List<Bid> bid { get; set; }
    }
}
