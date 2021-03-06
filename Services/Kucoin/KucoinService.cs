﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ZeegZag.Crawler2.Services.Database;
using ZeegZag.Data.Entity;

namespace ZeegZag.Crawler2.Services.Kucoin
{
    public class KucoinService : ExchangeServiceBase
    {
        private const string OrderUrl = "https://api.kucoin.com/v1/open/orders?symbol={0}-{1}&limit=10";
        private const string PriceUrl = "https://api.kucoin.com/v1/open/deal-orders?symbol={0}-{1}&limit=10";
        private const string MarketUrl = "https://api.kucoin.com/v1/market/open/symbols";
        private const string HealthUrl = "https://api.kucoin.com/v1/market/open/coin-info?coin={0}";
        

        public override string Name { get; } = "Kucoin";

        /// <inheritdoc />
        public override void Init(admin_zeegzagContext db)
        {
            GetExchangeId(db);
            RegisterRateLimit(18);

            CreatePuller(PULLER_CURRENCY, 60 * 60, 0, OnPullingCurrencies); //pull new currencies every 6 hours            
            CreatePuller(PULLER_MARKETS, 60 * 60, 45, OnPullingMarkets); //pull markets every hour            
            CreatePuller(PULLER_PRICE, 60, 90, OnPullingPrices); //pull prices every minute
            CreatePuller(PULLER_ORDER, 60, 90, OnPullingOrders); //pull orders every minute
            CreatePuller("puller-health", 60, 90, OnPullingHealth); //pull orders every minute
        }

        private async Task OnPullingHealth(PullerSession session)
        {
            ConcurrentDictionary<string, KucoinResponse<KucoinHealth>> cache = new ConcurrentDictionary<string, KucoinResponse<KucoinHealth>>();
            using (var db = DatabaseService.CreateContext())
            {
                //get all prices from db
                var prices = db.BorsaCurrencyT
                    .Where(bc => bc.BorsaId == ExchangeId && !bc.Disabled && bc.AutoGenerated != true)
                    .Select(bc => new
                    {
                        Id = bc.Id,
                        To = bc.ToCurrencyName,
                    }).ToList();
                int i = 0;

                ParallelFor(prices, bc =>
                {
                    KucoinResponse<KucoinHealth> health = cache.GetOrAdd(bc.To,
                        s => Pull<KucoinResponse<KucoinHealth>>(string.Format(HealthUrl, bc.To), session));
                    
                    if (health != null)
                    {
                        DatabaseService.Enqueue(new PriceUpdaterJob(bc.Id)
                            .DontUpdateDate()
                            .UpdateHealth(health.data.enableDeposit, health.data.enableWithdraw));
                    }

                }).Wait();
            }
        }
        private async Task OnPullingPrices(PullerSession session)
        {
            using (var db = DatabaseService.CreateContext())
            {
                //get all prices from db
                var prices = db.BorsaCurrencyT
                    .Where(bc => bc.BorsaId == ExchangeId && !bc.Disabled && bc.AutoGenerated != true)
                    .Select(bc => new
                    {
                        Id = bc.Id,
                        FromId = bc.FromCurrencyId,
                        From = bc.FromCurrencyName,
                        ToId = bc.ToCurrencyId,
                        To = bc.ToCurrencyName,
                        Price = bc.Price,
                    }).ToList();
                int i = 0;

                ParallelFor(prices, bc =>
                {
                    var responsePrice = Pull<KucoinResponse<List<List<object>>>>(string.Format(PriceUrl, bc.To, bc.From), session);

                    var last = responsePrice.data.Last();
                    var job = new PriceUpdaterJob(bc.Id, Convert.ToDecimal(last[2]));
                    var mv = Convert.ToDecimal(last[3]);
                    
                    job.UpdateVolume(mv, 1);

                    DatabaseService.Enqueue(job);
                    DatabaseService.Enqueue(new UsdGeneratorJob(ExchangeId, bc.FromId, bc.ToId, bc.Price));
                }).Wait();
            }
        }
        private async Task OnPullingOrders(PullerSession session)
        {
            using (var db = DatabaseService.CreateContext())
            {
                //get all prices from db
                var prices = db.BorsaCurrencyT
                    .Where(bc => bc.BorsaId == ExchangeId && !bc.Disabled && bc.AutoGenerated != true)
                    .Select(bc => new
                    {
                        Id = bc.Id,
                        FromId = bc.FromCurrencyId,
                        From = bc.FromCurrencyName,
                        ToId = bc.ToCurrencyId,
                        To = bc.ToCurrencyName,
                        Price = bc.Price,
                    }).ToList();
                

                ParallelFor(prices, bc =>
                {
                    var responseOrders = Pull<KucoinResponse<KucoinOrders>>(string.Format(OrderUrl, bc.To, bc.From), session);
                    DatabaseService.Enqueue(new OrderUpdaterJob(bc.Id)
                        .UpdateBuy(responseOrders.data.BUY, c => c[0], c => c[1])
                        .UpdateSell(responseOrders.data.SELL, c => c[0], c => c[1]));
                }).Wait();
            }
        }

        private async Task OnPullingMarkets(PullerSession session)
        {
            var response = Pull<KucoinResponse<List<KucoinMarket>>>(MarketUrl, session);

            using (var db = DatabaseService.CreateContext())
            {
                //get all prices from db
                var prices = db.BorsaCurrencyT
                    .Include(bc => bc.FromCurrency).Include(bc => bc.ToCurrency)
                    .Where(bc => bc.BorsaId == ExchangeId)
                    .ToList();

                //get markets
                foreach (var market in response.data)
                {
                    var from = market.coinTypePair;
                    var to = market.coinType;

                    new MarketUpdaterJob(ExchangeId, from, to, true, prices).Execute(db);
                }
                new MarketUpdaterJob(prices).Execute(db);

                db.SaveChanges();
            }
        }

        private async Task OnPullingCurrencies(PullerSession session)
        {
            var response = Pull<KucoinResponse<List<KucoinMarket>>>(MarketUrl, session);

            //get coin names
            var coinNames = response.data.SelectMany(p =>
                {
                    var from = p.coinTypePair;
                    var to = p.coinType;
                return new List<string>() { from, to };
            }).Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToList();

            foreach (var coin in coinNames)
            {
                DatabaseService.Enqueue(new CurrencyUpdaterJob(
                    ExchangeId,
                    coin,
                    coin,
                    true,
                    null));
            }
        }
        
        
    }
}
