using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZeegZag.Crawler2.Entity;

namespace ZeegZag.Crawler2.Services.Database
{
    public class PriceUpdaterJob : IDatabaseJob
    {
        private readonly int _bcId;
        private readonly decimal _price;
        private readonly decimal _volume;
        private readonly int _volumePeriod;
        private readonly decimal _volume24;
        private readonly decimal _volume24To;
        private bool updatePrice;
        private bool updateVolume;
        private bool updateVolume24;

        //price only cons
        public PriceUpdaterJob(
            int bcId,
            decimal price)
        {
            _bcId = bcId;
            _price = price;
            updatePrice = true;
        }

        //price + volume cons
        public PriceUpdaterJob(
            int bcId,
            decimal price,
            decimal volume,
            int volumePeriod)
        {
            _bcId = bcId;
            _price = price;
            _volume = volume;
            _volumePeriod = volumePeriod;
            updatePrice = true;
            updateVolume = true;
        }

        //price + volume + volume24 cons
        public PriceUpdaterJob(
            int bcId,
            decimal price,
            decimal volume,
            int volumePeriod,
            decimal volume24,
            decimal volume24To)
        {
            _bcId = bcId;
            _price = price;
            _volume = volume;
            _volumePeriod = volumePeriod;
            _volume24 = volume24;
            _volume24To = volume24To;
            updatePrice = true;
            updateVolume = true;
            updateVolume24 = true;
        }
        public void Execute(zeegzagContext db)
        {
            var bc = db.BorsaCurrencyT.Find(_bcId);

            //save history

            if (bc.LastUpdate.HasValue)
                db.HistoryT.Add(new HistoryT()
                {
                    BorsaCurrencyId = bc.Id,
                    EntryDate = bc.LastUpdate.Value,
                    Price = bc.Price,
                    High24Hour = bc.High24Hour,
                    Low24Hour = bc.Low24Hour,
                    Open24Hour = bc.Open24Hour,
                    Volume24Hour = bc.Volume24Hour,
                    Volume24HourTo = bc.Volume24HourTo
                });

            bc.LastUpdate = DateTime.Now;

            if (updatePrice)
                bc.Price = _price;
            if (updateVolume)
            {
                bc.Volume = _volume;
                bc.VolumePeriod = _volumePeriod;
            }
            if (updateVolume24)
            {
                bc.Volume24Hour = _volume24;
                bc.Volume24HourTo = _volume24To;
            }


        }
    }
}
