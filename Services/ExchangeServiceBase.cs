using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using ZeegZag.Crawler2.Entity;

namespace ZeegZag.Crawler2.Services
{
    /// <summary>
    /// Service base for all exchange services
    /// </summary>
    public abstract class ExchangeServiceBase
    {
        public int ExchangeId
        {
            get { return _exchangeId; }
        }

        /// <summary>
        /// Exchange name
        /// </summary>
        public abstract string Name { get; }

        protected const string PULLER_PRICE = "puller-price";
        protected const string PULLER_MARKETS = "puller-markets";
        protected const string PULLER_CURRENCY = "puller-currency";
        protected const string PULLER_VOLUME = "puller-volume";

        protected Dictionary<string, Puller> Pullers = new Dictionary<string, Puller>();
        private int _exchangeId;

        /// <summary>
        /// Initiates the exchange service
        /// </summary>
        public abstract void Init(zeegzagContext db);

        /// <summary>
        /// Must be called from exchange service from Init() method with its api name to get id and create if not exists
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="name">Name of exhange service</param>
        protected void GetExchangeId(zeegzagContext db)
        {
            var borsa = db.BorsaT.FirstOrDefault(b => b.Name == Name);
            if (borsa == null)
            {
                borsa = db.BorsaT.Add(new BorsaT()
                {
                    Name = Name,
                    ApiName = Name,
                    IsActive = true,
                }).Entity;
                db.SaveChanges();
            }
            _exchangeId = borsa.Id;
        }

        /// <summary>
        /// Creates a timer with given interval that will work to pull data from exchange markets
        /// </summary>
        /// <param name="name">Name of the puller</param>
        /// <param name="seconds">interval in seconds</param>
        /// <param name="runNow">Run this task immediately</param>
        /// <param name="onPullingHandler">Handler to be run while pulling</param>
        protected void CreatePuller(string name, int seconds, bool runNow, Func<Task> onPullingHandler)
        {
            var timer = new Puller(seconds*1000);
            timer.Name = name;
            ElapsedEventHandler onTimerElapsed = async (s, a) =>
            {
                try
                {
                    Console.WriteLine(Name + " " + ((Puller)s).Name + ": Running...");
                    await onPullingHandler();
                    Console.WriteLine(Name + " " + ((Puller)s).Name + ": Done");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            };
            timer.Elapsed += onTimerElapsed;
            Pullers[name] = timer;
            timer.Start();
            if (runNow)
                onTimerElapsed.Invoke(timer, null);
        }
        

        /// <summary>
        /// Sends a GET request to the given url and returns the json result as object
        /// </summary>
        /// <typeparam name="T">Return object type</typeparam>
        /// <param name="url">Url to request</param>
        /// <param name="configurationFactory">Can be used to configure client before sending request</param>
        /// <returns></returns>
        protected async Task<T> Pull<T>(string url, Action<HttpClient> configurationFactory = null)
        {
            using (HttpClient client = new HttpClient())
            {
                configurationFactory?.Invoke(client);
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
        }

        public DateTime TimestampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public static double DateTimeToTimestamp(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                    new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

    }
}