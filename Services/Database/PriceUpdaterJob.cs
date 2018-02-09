using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZeegZag.Data.Entity;

namespace ZeegZag.Crawler2.Services.Database
{
    public class PriceUpdaterJob : IDatabaseJob
    {
        private readonly int _bcId;
        private decimal _price;
        private decimal _volume;
        private int _volumePeriod;
        private decimal _volume24;
        private decimal _volume24To;
        private decimal _open;
        private decimal _high;
        private decimal _low;
        private decimal _close;
        private decimal _open24;
        private decimal _high24;
        private decimal _low24;
        private decimal _close24;
        private bool _canDeposit;
        private bool _canWithdraw;
        private bool updatePrice;
        private bool updateVolume;
        private bool updateVolume24;
        private bool updatePriceData;
        private bool updatePrice24Data;
        private bool updateHealth;
        private bool dontUpdateDate;

        public PriceUpdaterJob(
            int bcId)
        {
            _bcId = bcId;
        }

        //price only cons
        public PriceUpdaterJob(
            int bcId,
            decimal price)
        {
            _bcId = bcId;
            _price = Maximize(price);
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
            _price = Maximize(price);
            _volume = Maximize(volume);
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
            _price = Maximize(price);
            _volume = Maximize(volume);
            _volumePeriod = volumePeriod;
            _volume24 = Maximize(volume24);
            _volume24To = Maximize(volume24To);
            updatePrice = true;
            updateVolume = true;
            updateVolume24 = true;
        }

        public PriceUpdaterJob UpdateVolume(
            decimal volume,
            int volumePeriod)
        {
            _volume = Maximize(volume);
            _volumePeriod = volumePeriod;
            updateVolume = true;
            return this;
        }
        public PriceUpdaterJob UpdatePriceData(
            decimal open,
            decimal high,
            decimal low,
            decimal close)
        {
            _open = Maximize(open);
            _high = Maximize(high);
            _low = Maximize(low);
            _close = Maximize(close);
            updatePriceData = true;
            return this;
        }
        public PriceUpdaterJob UpdatePrice24Data(
            decimal open24,
            decimal high24,
            decimal low24,
            decimal close24)
        {
            _open24 = Maximize(open24);
            _high24 = Maximize(high24);
            _low24 = Maximize(low24);
            _close24 = Maximize(close24);
            updatePrice24Data = true;
            return this;
        }

        public PriceUpdaterJob UpdateVolume24(
            decimal volume24,
            decimal volume24To)
        {
            _volume24 = Maximize(volume24);
            _volume24To = Maximize(volume24To);
            updateVolume24 = true;
            return this;
        }

        public PriceUpdaterJob UpdateHealth(bool canDeposit, bool canWithdraw)
        {
            _canDeposit = canDeposit;
            _canWithdraw = canWithdraw;
            updateHealth = true;
            return this;
        }

        public PriceUpdaterJob DontUpdateDate()
        {
            dontUpdateDate = true;
            return this;
        }
        public void Execute(admin_zeegzagContext db)
        {
            var bc = db.BorsaCurrencyT.Find(_bcId);

            //save history

            if (!dontUpdateDate)
            {
                if (bc.LastUpdate.HasValue)
                    db.HistoryT.Add(new HistoryT()
                    {
                        BorsaCurrencyId = bc.Id,
                        EntryDate = bc.LastUpdate.Value,
                        Price = bc.Price,
                        High24Hour = bc.High24Hour,
                        Low24Hour = bc.Low24Hour,
                        Open24Hour = bc.Open24Hour,
                        Close24Hour = bc.Close24Hour,
                        Volume24Hour = bc.Volume24Hour,
                        Volume24HourTo = bc.Volume24HourTo,
                        Volume = bc.Volume,
                        VolumePeriod = bc.VolumePeriod,
                        Open = bc.Open,
                        Close = bc.Close,
                        High = bc.High,
                        Low = bc.Low,

                    });

                bc.LastUpdate = DateTime.Now;
            }

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
            if (updatePriceData)
            {
                bc.Open = _open;
                bc.High = _high;
                bc.Low = _low;
                bc.Close = _close;
            }
            if (updatePrice24Data)
            {
                bc.Open24Hour = _open24;
                bc.High24Hour = _high24;
                bc.Low24Hour = _low24;
                bc.Close24Hour = _close24;
            }
            if (updateHealth)
            {
                bc.CanDeposit = _canDeposit;
                bc.CanWithdraw = _canWithdraw;
            }

        }
        public override string ToString()
        {
            return "pu:" + _bcId;
        }

        decimal Maximize(decimal d)
        {
            return d > 99999999.99999999m ? 99999999.99999999m : d;
        }
    }
}
