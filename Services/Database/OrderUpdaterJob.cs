using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZeegZag.Data.Entity;

namespace ZeegZag.Crawler2.Services.Database
{
    public class OrderUpdaterJob : IDatabaseJob
    {
        private readonly int _bcId;
        private decimal? _buyArbitrageWeight;
        private decimal? _buyArbitrageVolume;
        private decimal? _sellArbitrageWeight;
        private decimal? _sellArbitrageVolume;
        private decimal? _buyPrice;
        private decimal? _sellPrice;

        public OrderUpdaterJob(
            int bcId)
        {
            _bcId = bcId;
        }
        
        public OrderUpdaterJob(
            int bcId,
            decimal? buyArbitrageWeight,
            decimal? buyArbitrageVolume,
            decimal? sellArbitrageWeight,
            decimal? sellArbitrageVolume,
            decimal? buyPrice,
            decimal? sellPrice)
        {
            _bcId = bcId;
            _buyArbitrageWeight = buyArbitrageWeight;
            _buyArbitrageVolume = buyArbitrageVolume;
            _sellArbitrageWeight = sellArbitrageWeight;
            _sellArbitrageVolume = sellArbitrageVolume;
            _buyPrice = buyPrice;
            _sellPrice = sellPrice;
        }

        public OrderUpdaterJob UpdateBuy<T>(List<T> list, Func<T, decimal?> priceFactory, Func<T, decimal?> volumeFactory)
        {
            _buyArbitrageVolume = list.Sum(volumeFactory);
            _buyArbitrageWeight = _buyArbitrageVolume > 0 ? list.Sum(t => priceFactory(t) * volumeFactory(t)) / _buyArbitrageVolume : 0;
            _buyPrice = list.Average(priceFactory);
            return this;
        }
        public OrderUpdaterJob UpdateSell<T>(List<T> list, Func<T, decimal?> priceFactory, Func<T, decimal?> volumeFactory)
        {
            _sellArbitrageVolume = list.Sum(volumeFactory);
            _sellArbitrageWeight = _sellArbitrageVolume > 0 ? list.Sum(t => priceFactory(t) * volumeFactory(t)) / _sellArbitrageVolume : 0;
            _sellPrice = list.Average(priceFactory);
            return this;
        }
        public void Execute(admin_zeegzagContext db)
        {
            var bc = db.BorsaCurrencyT.Find(_bcId);            
            
            bc.BuyArbitrageVolume = Maximize(_buyArbitrageVolume);
            bc.BuyArbitrageWeight = Maximize(_buyArbitrageWeight);
            bc.SellArbitrageVolume = Maximize(_sellArbitrageVolume);
            bc.SellArbitrageWeight = Maximize(_sellArbitrageWeight);
            bc.BuyPrice = Maximize(_buyPrice);
            bc.SellPrice = Maximize(_sellPrice);


        }
        public override string ToString()
        {
            return "ou:" + _bcId;
        }

        decimal? Maximize(decimal? d)
        {
            return d.HasValue ? (d > 99999999.99999999m ? 99999999.99999999m : d) : null;
        }
    }
}
