using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services.Bittrex
{
    public class Health
    {
        public string Currency { get; set; }
        public int DepositQueueDepth { get; set; }
        public int WithdrawQueueDepth { get; set; }
        public int BlockHeight { get; set; }
        public double WalletBalance { get; set; }
        public int WalletConnections { get; set; }
        public int MinutesSinceBHUpdated { get; set; }
        public DateTime LastChecked { get; set; }
        public bool IsActive { get; set; }
    }

    public class CurrencyObj
    {
        public string Currency { get; set; }
        public string CurrencyLong { get; set; }
        public int MinConfirmation { get; set; }
        public double TxFee { get; set; }
        public bool IsActive { get; set; }
        public string CoinType { get; set; }
        public string BaseAddress { get; set; }
        public string Notice { get; set; }
    }

    public class BittrexCurrencyHealth
    {
        public Health Health { get; set; }
        public CurrencyObj Currency { get; set; }
    }
}
