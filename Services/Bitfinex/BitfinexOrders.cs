using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Bitfinex
{
    public class Bid
    {
        public string price { get; set; }
        public string amount { get; set; }
        public string timestamp { get; set; }
    }

    public class Ask
    {
        public string price { get; set; }
        public string amount { get; set; }
        public string timestamp { get; set; }
    }

    public class BitfinexOrders
    {
        public List<Bid> bids { get; set; }
        public List<Ask> asks { get; set; }
    }
}
