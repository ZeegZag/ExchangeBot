﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ZeegZag.Crawler2.Services.Database;
using ZeegZag.Crawler2.Services.Poloniex;
using ZeegZag.Data.Entity;

namespace ZeegZag.Crawler2.Services.Yobit
{
    public class YobitService : ExchangeServiceBase
    {
        private const string CurrencyMarketUrl = "https://yobit.net/api/3/info";
        private const string PriceUrl = "https://yobit.net/api/3/ticker/{0}_{1}";

        /// <inheritdoc />
        public override string Name { get; } = "Yobit";
        public override bool IsDisabled { get; } = true;

        /// <inheritdoc />
        public override void Init(admin_zeegzagContext db)
        {
            GetExchangeId(db);
            RegisterRateLimit(6);

            CreatePuller(PULLER_CURRENCY, 60 * 60 * 6, 0, OnPullingCurrencies); //pull new currencies every 6 hours            
            CreatePuller(PULLER_MARKETS, 60 * 60, 45, OnPullingMarkets); //pull markets every hour            
            CreatePuller(PULLER_PRICE, 60, 90, OnPullingPrices); //pull prices every minute
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


                Parallel.ForEach(prices, bc =>
                {

                    var response = Pull<JToken>(string.Format(PriceUrl, bc.To.ToLower(), bc.From.ToLower()), session);
                    var data = response.Children<JProperty>().First().Value.ToObject<YobitTicker>();

                    DatabaseService.Enqueue(new PriceUpdaterJob(bc.Id, data.last).UpdateVolume24(data.vol, data.vol_cur));
                    DatabaseService.Enqueue(new UsdGeneratorJob(ExchangeId, bc.FromId, bc.ToId, bc.Price));
                });
            }
        }

        private async Task OnPullingMarkets(PullerSession session)
        {
            var response = Pull<YobitResponse>(CurrencyMarketUrl, session);

            using (var db = DatabaseService.CreateContext())
            {

                //get all prices from db
                var prices = db.BorsaCurrencyT
                    .Include(bc => bc.FromCurrency).Include(bc => bc.ToCurrency)
                    .Where(bc => bc.BorsaId == ExchangeId)
                    .ToList();

                foreach (var obj in response.pairs.Children<JProperty>())
                {
                    var data = obj.Value.ToObject<YobitMarket>();
                    var fromTo = obj.Name.Split('_');
                    var from = fromTo[1].ToUpper();
                    var to = fromTo[0].ToUpper();

                    new MarketUpdaterJob(ExchangeId, from, to, data.hidden == 0, prices).Execute(db);
                }

                new MarketUpdaterJob(prices).Execute(db);

                db.SaveChanges();
            }
        }

        private async Task OnPullingCurrencies(PullerSession session)
        {
            var response = Pull<YobitResponse>(CurrencyMarketUrl, session);


            //get coin names
            var coinNames = response.pairs.Children<JProperty>().SelectMany(p =>
                {
                    var data = p.Value.ToObject<YobitMarket>();
                    var fromTo = p.Name.Split('_');
                    var from = fromTo[1].ToUpper();
                    var to = fromTo[0].ToUpper();
                    return new[] { Tuple.Create(from, data.fee, data.hidden == 0), Tuple.Create(to, data.fee, data.hidden == 0) };
                }).Where(c => !string.IsNullOrEmpty(c.Item1))
                .Distinct()
                .ToList();

            foreach (var obj in coinNames)
            {
                DatabaseService.Enqueue(new CurrencyUpdaterJob(
                    ExchangeId,
                    obj.Item1,
                    obj.Item1,
                    obj.Item3,
                    obj.Item2));
            }
        }
    }
}
