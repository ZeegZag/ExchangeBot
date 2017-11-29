﻿using System;
using System.Collections.Generic;

namespace ZeegZag.Crawler2.Entity
{
    public partial class BorsaCurrencyT
    {
        public BorsaCurrencyT()
        {
            HistoryT = new HashSet<HistoryT>();
        }

        public int Id { get; set; }
        public int BorsaId { get; set; }
        public int FromCurrencyId { get; set; }
        public decimal Price { get; set; }
        public int ToCurrencyId { get; set; }
        public bool Disabled { get; set; }
        public DateTime? LastUpdate { get; set; }
        public bool? AutoGenerated { get; set; }
        public decimal? Open24Hour { get; set; }
        public decimal? High24Hour { get; set; }
        public decimal? Low24Hour { get; set; }
        public decimal? OpenHour { get; set; }
        public decimal? HighHour { get; set; }
        public decimal? LowHour { get; set; }
        public decimal? VolumeHour { get; set; }
        public decimal? Volume24Hour { get; set; }
        public decimal? Volume24HourTo { get; set; }
        public decimal? Volume { get; set; }
        public int? VolumePeriod { get; set; }
        public decimal? TxFee { get; set; }

        public BorsaT Borsa { get; set; }
        public CurrencyT FromCurrency { get; set; }
        public CurrencyT ToCurrency { get; set; }
        public ICollection<HistoryT> HistoryT { get; set; }
    }
}
