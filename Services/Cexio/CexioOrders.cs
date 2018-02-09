using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Cexio
{
    public class CexioOrders
    {
        public long timestamp { get; set; }
        public List<List<decimal>> bids { get; set; }
        public List<List<decimal>> asks { get; set; }
        public string pair { get; set; }
        public long id { get; set; }
        public string sell_total { get; set; }
        public string buy_total { get; set; }
    }
}
