using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Cexio
{
    public class CexioPrice
    {
        public string timestamp { get; set; }
        public string pair { get; set; }
        public string low { get; set; }
        public string high { get; set; }
        public string last { get; set; }
        public string volume { get; set; }
        public string volume30d { get; set; }
        public double bid { get; set; }
        public double ask { get; set; }
    }
}
