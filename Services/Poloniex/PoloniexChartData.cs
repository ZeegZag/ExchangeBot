using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Poloniex
{
    public class PoloniexChartData
    {
        public int date { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal open { get; set; }
        public decimal close { get; set; }
        public decimal volume { get; set; }
        public decimal quoteVolume { get; set; }
        public decimal weightedAverage { get; set; }
    }
}
