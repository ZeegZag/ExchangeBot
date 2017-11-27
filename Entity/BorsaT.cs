using System;
using System.Collections.Generic;

namespace ZeegZag.Crawler2.Entity
{
    public partial class BorsaT
    {
        public BorsaT()
        {
            BorsaCurrencyT = new HashSet<BorsaCurrencyT>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ApiName { get; set; }
        public bool IsActive { get; set; }
        public bool UseUsd { get; set; }

        public ICollection<BorsaCurrencyT> BorsaCurrencyT { get; set; }
    }
}
