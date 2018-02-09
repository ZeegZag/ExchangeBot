using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Kucoin
{
    public class KucoinMarket
    {
        public string coinType { get; set; }
        public bool trading { get; set; }
        public string symbol { get; set; }
        public decimal lastDealPrice { get; set; }
        public decimal buy { get; set; }
        public decimal sell { get; set; }
        public decimal? change { get; set; }
        public string coinTypePair { get; set; }
        public int sort { get; set; }
        public decimal feeRate { get; set; }
        public decimal volValue { get; set; }
        public decimal high { get; set; }
        public object datetime { get; set; }
        public decimal vol { get; set; }
        public decimal low { get; set; }
        public decimal changeRate { get; set; }
    }
}
