﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ZeegZag.Crawler2.Entity;
using ZeegZag.Crawler2.Services.Database;

namespace ZeegZag.Crawler2.Services.Binance
{
    public class BinanceService : ExchangeServiceBase
    {
        private const string PriceUrl = "https://api.binance.com/api/v1/ticker/allPrices";
        private const string MinuteVolumeUrl = "https://api.binance.com/api/v1/klines?symbol={0}&interval=1m&limit=1";
        private const string DailyVolumeUrl = "https://api.binance.com/api/v1/klines?symbol={0}&interval=1d&limit=1";

        public override string Name { get; } = "Binance";
        public override bool IsDisabled { get; } = false;

        /// <inheritdoc />
        public override void Init(zeegzagContext db)
        {
            GetExchangeId(db);
            RegisterRateLimit(6);
            AlignSockets(5);

            CreatePuller(PULLER_CURRENCY, 60 * 60, 0, OnPullingCurrencies); //pull new currencies every 6 hours            
            CreatePuller(PULLER_MARKETS, 60 * 60, 45, OnPullingMarkets); //pull markets every hour            
            CreatePuller(PULLER_PRICE, 60, 90, OnPullingPrices); //pull prices every minute
        }

        private async Task OnPullingPrices()
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

                var response = Pull<List<BinancePrice>>(PriceUrl).Result;

                var tasks = new List<Task>(prices.Count);
                foreach (var bc in prices)
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        var symbol = bc.To + bc.From;
                        var foundPrice = response.FirstOrDefault(p => p.symbol == symbol);
                        if (foundPrice != null)
                        {
                            var job = new PriceUpdaterJob(bc.Id, Convert.ToDecimal(foundPrice.price));



                            //request last minute and daily ticker

                            var responseMinute = Pull<JArray[]>(string.Format(MinuteVolumeUrl, symbol)).Result;
                            //var responseDaily = Pull<JArray[]>(string.Format(DailyVolumeUrl, symbol)).Result;

                            if (responseMinute.Length > 0)
                            {
                                var minuteData = responseMinute[0];
                                //var dailyData = responseDaily[0];
                                var mv = Convert.ToDecimal(minuteData[5].Value<string>());
                                //var dv = Convert.ToDecimal(dailyData[5].Value<string>());
                                //var dbv = Convert.ToDecimal(dailyData[7].Value<string>());
                                //var open = Convert.ToDecimal(dailyData[1].Value<string>());
                                //var high = Convert.ToDecimal(dailyData[2].Value<string>());
                                //var low = Convert.ToDecimal(dailyData[3].Value<string>());
                                //var close = Convert.ToDecimal(dailyData[4].Value<string>());
                                job.UpdateVolume(mv, 1);
                                //job.UpdatePriceData(open, high, low, close);
                                //job.UpdateVolume24(dv, dbv);
                            }
                            else
                            {
                                Console.WriteLine(string.Format("Could not pull {0}-{1} volume from {2}: List was empty",
                                    bc.To, bc.From, Name));
                            }

                            DatabaseService.Enqueue(job);
                            DatabaseService.Enqueue(new UsdGeneratorJob(ExchangeId, bc.FromId, bc.ToId, bc.Price));
                        }
                    }));
                }
                Task.WaitAll(tasks.ToArray());
            }
        }

        private async Task OnPullingMarkets()
        {
            var response = Pull<List<BinancePrice>>(PriceUrl).Result;

            using (var db = DatabaseService.CreateContext())
            {
                //get all prices from db
                var prices = db.BorsaCurrencyT
                    .Include(bc => bc.FromCurrency).Include(bc => bc.ToCurrency)
                    .Where(bc => bc.BorsaId == ExchangeId)
                    .ToList();

                //get markets
                foreach (var market in response)
                {
                    var isUsdt = market.symbol.EndsWith("USDT");
                    var from = market.symbol.Substring(market.symbol.Length - (isUsdt ? 4 : 3));
                    var to = market.symbol.Substring(0, market.symbol.Length - (isUsdt ? 4 : 3));

                    new MarketUpdaterJob(ExchangeId, from, to, true, prices).Execute(db);
                }
                new MarketUpdaterJob(prices).Execute(db);

                db.SaveChanges();
            }
        }

        private async Task OnPullingCurrencies()
        {
            var response = Pull<List<BinancePrice>>(PriceUrl).Result;

            //get coin names
            var coinNames = response.SelectMany(p =>
            {
                var isUsdt = p.symbol.EndsWith("USDT");
                var from = p.symbol.Substring(p.symbol.Length - (isUsdt ? 4 : 3));
                var to = p.symbol.Substring(0, p.symbol.Length - (isUsdt ? 4 : 3));
                return new List<string>(){from, to};
            }).Where(c => !string.IsNullOrEmpty(c) && c != "123" && c != "456")
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
