﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ZeegZag.Crawler2.Services.Database;
using ZeegZag.Data.Entity;

namespace ZeegZag.Crawler2.Services
{
    public class VolumeUpdateService : ExchangeServiceBase
    {
        public override string Name { get; } = "Volumer";
        public override void Init(admin_zeegzagContext db)
        {
            ExchangeId = 0;
            CreatePuller("volumer",60 * 10, 120, OnVoluming, false);
            //CreatePuller("clear-history",60, 120 + (60*5), OnClearHistory, false);
            CreatePuller("arbitrage",60, 120, OnCalculateArbitrage, false);
        }

        private async Task OnVoluming(PullerSession session)
        {
            using (var db = DatabaseService.CreateContext())
            {
                db.Database.ExecuteSqlCommand(
                    "UPDATE borsa_currency_t A SET Volume24Hour = (SELECT LEAST(SUM(B.Volume / GREATEST(B.VolumePeriod,1)), 99999999.99999999) FROM history_t B WHERE B.BorsaCurrencyId = A.Id AND B.EntryDate > DATE_SUB(NOW(), INTERVAL 1 DAY)) WHERE NOT Disabled = 1 AND NOT AutoGenerated = 1 AND Volume > 0");                
            }
        }
        private async Task OnClearHistory(PullerSession session)
        {
            using (var db = DatabaseService.CreateContext())
            {
                //clear old history
                var date = DateTime.Now.AddDays(-2);
                db.Database.ExecuteSqlCommand("DELETE FROM history_t WHERE EntryDate < '" +
                                              date.ToString("yyyy-MM-dd HH:mm:ss") + "'");
            }
        }
        private async Task OnCalculateArbitrage (PullerSession session)
        {
            using (var db = DatabaseService.CreateContext())
            {
                var lastDate = DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss");
                var sql = $@"SELECT * FROM (SELECT T1.*, T2.*, FC.ShortName FromCoin, TC.ShortName ToCoin, ((T1.BuyArbitrageWeight - T2.SellArbitrageWeight) / T2.SellArbitrageWeight) * 100 AS ChangeRate FROM 
(SELECT BC.Id, BC.CanWithdraw, BC.CanDeposit, BC.Price, BC.BuyArbitrageWeight, BC.BuyArbitrageVolume, BC.SellArbitrageVolume, BC.BorsaId, BC.FromCurrencyId, BC.ToCurrencyId, B.Name BorsaName, BC.Volume24Hour FROM borsa_currency_t BC 
INNER JOIN borsa_t B ON BC.BorsaId = B.Id
WHERE (NOT BC.Disabled = TRUE) AND (NOT BC.AutoGenerated = TRUE) AND (BC.LastUpdate > '{lastDate}')
) T1 
INNER JOIN 
(SELECT BC.Id Id1, BC.CanWithdraw CanWithdraw1, BC.CanDeposit CanDeposit1, BC.Price Price1, BC.BuyArbitrageVolume BuyArbitrageVolume1, BC.SellArbitrageVolume SellArbitrageVolume1, BC.SellArbitrageWeight, BC.BorsaId BorsaId1, BC.FromCurrencyId FromCurrencyId1, BC.ToCurrencyId ToCurrencyId1, B.Name BorsaName1, BC.Volume24Hour Volume24Hour1 FROM borsa_currency_t BC 
INNER JOIN borsa_t B ON BC.BorsaId = B.Id
WHERE (NOT BC.Disabled = TRUE) AND (NOT BC.AutoGenerated = TRUE) AND (BC.LastUpdate > '{lastDate}')
) T2 ON
T1.ToCurrencyId = T2.ToCurrencyId1 AND T1.FromCurrencyId = T2.FromCurrencyId1 AND NOT T1.BorsaId = T2.BorsaId1
INNER JOIN currency_t TC ON T1.ToCurrencyId = TC.Id 
INNER JOIN currency_t FC ON T1.FromCurrencyId = FC.Id 
) TT
WHERE ChangeRate > 0";

                db.ArbitrageT.Load();
                var dic = db.ArbitrageT.Local.ToDictionary(a => Tuple.Create((int?)a.BuyerBcid, (int?)a.SellerBcid), a => a);
                using (var cmd = db.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    try
                    {
                        db.Database.OpenConnection();
                        using (var reader = cmd.ExecuteReader())
                        {                            
                            while (reader.Read())
                            {
                                var key = Tuple.Create(reader["Id1"] as int?, reader["Id"] as int?);
                                var entity = dic.ContainsKey(key)
                                    ? dic[key]
                                    : db.ArbitrageT.Add(new ArbitrageT()
                                    {
                                        FromBorsaId = (int)reader["BorsaId1"],
                                        ToBorsaId = (int)reader["BorsaId"],
                                        FromCurrencyId = (int)reader["FromCurrencyId1"],
                                        ToCurrencyId = (int)reader["ToCurrencyId1"],
                                    }).Entity;

                                if (entity.Id > 0)
                                {
                                    db.ArbitrageHistoryT.Add(new ArbitrageHistoryT()
                                    {
                                        BuyerBcid = entity.BuyerBcid,
                                        SellerBcid = entity.SellerBcid,
                                        FromCurrencyId = entity.FromCurrencyId,
                                        ToBorsaId = entity.ToBorsaId,
                                        ToCurrencyId = entity.ToCurrencyId,
                                        BuyerPrice = entity.BuyerPrice,
                                        BuyerBuyArbitrageVolume = entity.BuyerBuyArbitrageVolume,
                                        BuyerSellArbitrageVolume = entity.BuyerSellArbitrageVolume,
                                        BuyerSellArbitrageWeight = entity.BuyerSellArbitrageWeight,
                                        BuyerVolume24Hour = entity.BuyerVolume24Hour,
                                        ChangeRate = entity.ChangeRate,
                                        EntryDate = entity.EntryDate,
                                        FromBorsaId = entity.FromBorsaId,
                                        SellerBuyArbitrageVolume = entity.SellerBuyArbitrageVolume,
                                        SellerBuyArbitrageWeight = entity.SellerBuyArbitrageWeight,
                                        SellerSellArbitrageVolume = entity.SellerSellArbitrageVolume,
                                        SellerPrice = entity.SellerPrice,
                                        SellerVolume24Hour = entity.SellerVolume24Hour,
                                        isTradable = entity.isTradable
                                    });
                                }
                                entity.BuyerBcid = (int) reader["Id1"];
                                entity.BuyerPrice = (reader["Price1"] as decimal?).GetValueOrDefault(0);
                                entity.BuyerSellArbitrageWeight = (reader["SellArbitrageWeight"] as decimal?).GetValueOrDefault(0);
                                entity.BuyerBuyArbitrageVolume = (reader["BuyArbitrageVolume1"] as decimal?).GetValueOrDefault(0);
                                entity.BuyerSellArbitrageVolume = (reader["SellArbitrageVolume1"] as decimal?).GetValueOrDefault(0);
                                entity.BuyerVolume24Hour = reader["Volume24Hour1"] as decimal?;
                                entity.SellerBcid = (int)reader["Id"];
                                entity.SellerPrice = (reader["Price"] as decimal?).GetValueOrDefault(0);
                                entity.SellerBuyArbitrageWeight = (reader["BuyArbitrageWeight"] as decimal?).GetValueOrDefault(0);
                                entity.SellerBuyArbitrageVolume = (reader["BuyArbitrageVolume"] as decimal?).GetValueOrDefault(0);
                                entity.SellerSellArbitrageVolume = (reader["SellArbitrageVolume"] as decimal?).GetValueOrDefault(0);
                                entity.SellerVolume24Hour = reader["Volume24Hour"] as decimal?;
                                entity.ChangeRate = Math.Min(Math.Round((reader["ChangeRate"] as decimal?).GetValueOrDefault(0),4),999999.9999m);
                                entity.EntryDate = DateTime.Now;
                                entity.isTradable =
                                    !"0".Equals(reader["CanWithdraw1"].ToString()) && !"0".Equals(reader["CanDeposit1"].ToString()) &&
                                    !"0".Equals(reader["CanWithdraw"].ToString()) && !"0".Equals(reader["CanWithdraw"].ToString());
                                entity.FromCoinName = reader["FromCoin"].ToString();
                                entity.ToCoinName = reader["ToCoin"].ToString();
                                entity.FromBorsaName = reader["BorsaName1"].ToString();
                                entity.ToBorsaName = reader["BorsaName"].ToString();

                            }
                        }
                        db.Database.CloseConnection();
                    }
                    catch
                    {
                    }
                }
                db.SaveChanges();
            }
        }
    }

}
