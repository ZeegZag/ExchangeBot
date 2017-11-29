using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Bittrex
{
    public class BittrexResponse<T>
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<T> result { get; set; }
    }
}
