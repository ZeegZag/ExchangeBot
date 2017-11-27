using System;
using System.Collections.Generic;

namespace ZeegZag.Crawler2.Entity
{
    public partial class CurrencyT
    {
        public CurrencyT()
        {
            BorsaCurrencyTFromCurrency = new HashSet<BorsaCurrencyT>();
            BorsaCurrencyTToCurrency = new HashSet<BorsaCurrencyT>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string ImageUrl { get; set; }
        public string Alias { get; set; }

        public ICollection<BorsaCurrencyT> BorsaCurrencyTFromCurrency { get; set; }
        public ICollection<BorsaCurrencyT> BorsaCurrencyTToCurrency { get; set; }
    }
}
