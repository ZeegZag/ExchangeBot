using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Bittrex
{
    public class BittrexCurrency
    {
        public string Currency { get; set; }
        public string CurrencyLong { get; set; }
        public int MinConfirmation { get; set; }
        public decimal TxFee { get; set; }
        public bool IsActive { get; set; }
        public string CoinType { get; set; }
        public string BaseAddress { get; set; }
        public string Notice { get; set; }
    }
    
}
