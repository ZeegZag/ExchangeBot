using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Kucoin
{
    public class KucoinOrders
    {
        public List<List<decimal>> SELL { get; set; }
        public List<List<decimal>> BUY { get; set; }
    }
}
