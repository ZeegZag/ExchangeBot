using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services
{
    public class Puller : System.Timers.Timer
    {
        public Puller()
        {
        }

        public Puller(double interval) : base(interval)
        {
        }

        public string Name { get; set; }
    }
}
