using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Gdax
{
    public class GdaxMarket
    {
        public string id { get; set; }
        public string base_currency { get; set; }
        public string quote_currency { get; set; }
        public string base_min_size { get; set; }
        public string base_max_size { get; set; }
        public string quote_increment { get; set; }
        public string display_name { get; set; }
        public string status { get; set; }
        public bool margin_enabled { get; set; }
        public object status_message { get; set; }
    }
}
