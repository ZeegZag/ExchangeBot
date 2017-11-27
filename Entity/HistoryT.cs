using System;
using System.Collections.Generic;

namespace ZeegZag.Crawler2.Entity
{
    public partial class HistoryT
    {
        public int Id { get; set; }
        public int BorsaCurrencyId { get; set; }
        public decimal Price { get; set; }
        public DateTime EntryDate { get; set; }
        public decimal? Open24Hour { get; set; }
        public decimal? High24Hour { get; set; }
        public decimal? Low24Hour { get; set; }
        public decimal? VolumeHour { get; set; }
        public decimal? Volume24Hour { get; set; }
        public decimal? Volume24HourTo { get; set; }

        public BorsaCurrencyT BorsaCurrency { get; set; }
    }
}
