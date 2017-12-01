using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Poloniex
{
    public class PoloniexCurrency
    {
        public int id { get; set; }
        public string name { get; set; }
        public string txFee { get; set; }
        public int minConf { get; set; }
        public object depositAddress { get; set; }
        public int disabled { get; set; }
        public int delisted { get; set; }
        public int frozen { get; set; }
    }
}
