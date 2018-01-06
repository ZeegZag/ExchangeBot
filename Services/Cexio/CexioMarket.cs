using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Cexio
{
    public class CexioMarket
    {
        public string symbol1 { get; set; }
        public string symbol2 { get; set; }
        public double minLotSize { get; set; }
        public double minLotSizeS2 { get; set; }
        public int? maxLotSize { get; set; }
        public string minPrice { get; set; }
        public string maxPrice { get; set; }
    }
}
