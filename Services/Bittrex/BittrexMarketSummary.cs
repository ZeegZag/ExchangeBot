using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Bittrex
{
    public class BittrexMarketSummary
    {
        public BittrexMarket Market { get; set; }
        public BittrexSummary Summary { get; set; }
        public bool IsVerified { get; set; }
    }
}
