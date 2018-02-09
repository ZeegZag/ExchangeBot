using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Kucoin
{
    public class KucoinResponse<T>
    {
        public bool success { get; set; }
        public string code { get; set; }
        public string msg { get; set; }
        public long timestamp { get; set; }
        public T data { get; set; }
    }
}
