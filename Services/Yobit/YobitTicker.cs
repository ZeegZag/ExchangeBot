﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Yobit
{
    public class YobitTicker
    {
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal avg { get; set; }
        public decimal vol { get; set; }
        public decimal vol_cur { get; set; }
        public decimal last { get; set; }
        public decimal buy { get; set; }
        public decimal sell { get; set; }
        public int updated { get; set; }
    }
}
