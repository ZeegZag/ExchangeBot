using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Hitbtc
{
    public class HitbtcHealth
    {
        public string id { get; set; }
        public string fullName { get; set; }
        public bool crypto { get; set; }
        public bool payinEnabled { get; set; }
        public bool payinPaymentId { get; set; }
        public int payinConfirmations { get; set; }
        public bool payoutEnabled { get; set; }
        public bool payoutIsPaymentId { get; set; }
        public bool transferEnabled { get; set; }
    }
}
