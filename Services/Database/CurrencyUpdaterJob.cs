using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using ZeegZag.Data.Entity;

namespace ZeegZag.Crawler2.Services.Database
{
    /// <summary>
    /// Adds new currencies of exchange market and updates txfee values
    /// </summary>
    public class CurrencyUpdaterJob : IDatabaseJob
    {
        private readonly int _exchangeId;
        private readonly string _currencyShort;
        private readonly string _currencyLong;
        private readonly bool? _isActive;
        private readonly decimal? _txFee;

        /// <inheritdoc />
        public CurrencyUpdaterJob(int exchangeId, string currencyShort, string currencyLong, bool? isActive, decimal? txFee)
        {
            _exchangeId = exchangeId;
            _currencyShort = currencyShort.ToUpper();
            _currencyLong = currencyLong;
            _isActive = isActive;
            _txFee = txFee;
        }

        /// <inheritdoc />
        public void Execute(admin_zeegzagContext db)
        {
            var alias = ";" + _currencyShort + ";";
            var currencyId = CachingService.CurrencyIdByName(_currencyShort,
                () => db.CurrencyT.FirstOrDefault(c => c.ShortName == _currencyShort || c.Alias.Contains(alias))?.Id);

            if (!currencyId.HasValue)
            {
                if (_isActive.GetValueOrDefault(true))
                {
                    //add new coin (since we do not call savechanges() immediately, we do not have id yet. So txfee will be updated on next turn)
                    db.CurrencyT.Add(new CurrencyT()
                    {
                        Name = _currencyLong ?? _currencyShort,
                        ShortName = _currencyShort,
                    });
                    DatabaseService.EnqueueBroadcast(437907950, $"New coin is added to ZeegZag: {_currencyShort}", 
                        new InlineKeyboardMarkup(new InlineKeyboardButton[]{new InlineKeyboardUrlButton("Advertise here", "https://www.zeegzag.com"), }));
                }
            }
            else
            {
                //update txfee
                var borsaCurrency = db.BorsaCurrencyT
                    .Where(bc => bc.BorsaId == _exchangeId && bc.ToCurrencyId == currencyId).ToList();
                foreach (var bc in borsaCurrency)
                {
                    bc.TxFee = _txFee;
                    if (_isActive.HasValue)
                        bc.Disabled = !_isActive.Value;
                }
            }
        }

        public override string ToString()
        {
            return "cu:" + _exchangeId;
        }
    }
}
