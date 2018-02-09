using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Bittrex
{
    public class Buy
    {
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
    }

    public class Sell
    {
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
    }

    public class BittrexOrders
    {
        public List<Buy> buy { get; set; }
        public List<Sell> sell { get; set; }
    }
}
