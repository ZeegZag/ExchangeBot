using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Yobit
{
    public class YobitMarket
    {
        public int decimal_places { get; set; }
        public decimal min_price { get; set; }
        public int max_price { get; set; }
        public decimal min_amount { get; set; }
        public decimal min_total { get; set; }
        public int hidden { get; set; }
        public decimal fee { get; set; }
        public decimal fee_buyer { get; set; }
        public decimal fee_seller { get; set; }
    }
}
