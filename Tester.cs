using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZeegZag.Crawler2.Services;
using ZeegZag.Crawler2.Services.Binance;
using ZeegZag.Crawler2.Services.Bitfinex;
using ZeegZag.Crawler2.Services.Bittrex;
using ZeegZag.Crawler2.Services.Cexio;
using ZeegZag.Crawler2.Services.Cryptopia;
using ZeegZag.Crawler2.Services.Gdax;
using ZeegZag.Crawler2.Services.Hitbtc;
using ZeegZag.Crawler2.Services.Poloniex;
using ZeegZag.Crawler2.Services.Yobit;

#if DEBUG
namespace ZeegZag.Crawler2
{
    public static class Tester
    {
        /// <summary>
        /// List of services and pullers to be tested. Do not pass puller to test all pullers of this service
        /// </summary>
        private static readonly List<TestedService> ServicesToTest = new List<TestedService>
        {
            new TestedService(typeof(GdaxService)),
        };

        public static bool IsServiceTesting(ExchangeServiceBase service, string pullerName)
        {
            return true;
            return ServicesToTest.Any(s => s.IsServiceTesting(service, pullerName));
        }
        public static bool IsServiceTesting(ExchangeServiceBase service)
        {
            return true;
            return ServicesToTest.Any(s => s.IsServiceTesting(service));
        }
    }

    public class TestedService
    {
        private readonly Type _exchangeServiceType;
        private readonly string[] _pullers;

        public TestedService(Type exchangeServiceType, params string[] pullers)
        {
            _exchangeServiceType = exchangeServiceType;
            _pullers = pullers;
        }

        public bool IsServiceTesting<T>(T service, string pullerName) where T : ExchangeServiceBase
        {
            return service.GetType() == _exchangeServiceType &&
                   (_pullers.Length == 0 || _pullers.Contains(pullerName));
        }
        public bool IsServiceTesting<T>(T service) where T : ExchangeServiceBase
        {
            return service.GetType() == _exchangeServiceType;
        }
    }
}
#endif
