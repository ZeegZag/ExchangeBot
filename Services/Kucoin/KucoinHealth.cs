using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Kucoin
{
    public class KucoinHealth
    {
        public double withdrawMinFee { get; set; }
        public double withdrawMinAmount { get; set; }
        public double withdrawFeeRate { get; set; }
        public int confirmationCount { get; set; }
        public string withdrawRemark { get; set; }
        public object infoUrl { get; set; }
        public string name { get; set; }
        public int tradePrecision { get; set; }
        public object depositRemark { get; set; }
        public bool enableWithdraw { get; set; }
        public bool enableDeposit { get; set; }
        public string coin { get; set; }
    }
}
