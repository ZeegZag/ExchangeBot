using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
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
        private RateLimit RateLimit;
        public int ExchangeId
        {
            get { return _exchangeId; }
        }

        /// <summary>
        /// Exchange name
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Service is disabled
        /// </summary>
        public virtual bool IsDisabled { get; }

        public const string PULLER_PRICE = "puller-price";
        public const string PULLER_MARKETS = "puller-markets";
        public const string PULLER_CURRENCY = "puller-currency";
        public const string PULLER_VOLUME = "puller-volume";

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
        /// <param name="delaySeconds">Wait before starting puller</param>
        /// <param name="onPullingHandler">Handler to be run while pulling</param>      
        protected void CreatePuller(string name, int seconds, int delaySeconds, Func<Task> onPullingHandler)
        {
#if DEBUG
            if (!Tester.IsServiceTesting(this,name))
                return;
#endif
            Task.Factory.StartNew(() =>
            {
                if (delaySeconds>0)
                    Thread.Sleep(delaySeconds*1000);

                var timer = new Puller(seconds * 1000);
                timer.Name = name;
                ElapsedEventHandler onTimerElapsed = async (s, a) =>
                {
                    var puller = (Puller)s;
                    try
                    {
                        if (puller.IsPulling)
                        {
                            Console.WriteLine(Name + " " + puller.Name + ": Still running, skipping...");
                        }
                        else
                        {
                            Console.WriteLine(Name + " " + puller.Name + ": Running...");
                            puller.IsPulling = true;
                            await onPullingHandler().ConfigureAwait(false);
                            puller.IsPulling = false;
                            Console.WriteLine(Name + " " + puller.Name + ": Done");
                        }
                    }
                    catch (Exception e)
                    {
                        puller.IsPulling = false;
                        Console.WriteLine(e);
                    }
                };
                timer.Elapsed += onTimerElapsed;
                Pullers[name] = timer;
                timer.Start();
                onTimerElapsed.Invoke(timer, null);
            });
        }

        public void RegisterRateLimit(int requestLimitPerSecond, int? millisecondsBetween = null)
        {
            RateLimit = new RateLimit()
            {
                PassCountPerSecond = 0,
                Limit = requestLimitPerSecond,
                SecondsTimestamp = 0,
                WaitMillis = millisecondsBetween ?? (1000 / requestLimitPerSecond),
                MillisTimestamp = 0
            };
        }

        public void AlignSockets(int socketCount)
        {
            SocketManager.AlignForBorsa(ExchangeId, socketCount);
        }

        private int counter = 0;
        private readonly object locker = new object();

        public ExchangeServiceBase()
        {
            //Client.DefaultRequestHeaders.ConnectionClose = true;
            
            
        }

        /// <summary>
        /// Sends a GET request to the given url and returns the json result as object
        /// </summary>
        /// <typeparam name="T">Return object type</typeparam>
        /// <param name="url">Url to request</param>
        /// <param name="configurationFactory">Can be used to configure client before sending request</param>
        /// <returns></returns>
        protected async Task<T> Pull<T>(string url, params Tuple<string,string>[] headers)
        {
            RateLimitService.WaitForLimit(ExchangeId);
            try
            {
                lock (locker)
                {
                    counter = SocketManager.NextIndex(ExchangeId, counter);
                }
                int trial = 0;
                //url = $"http://zzproxy{counter}.eu-gb.mybluemix.net/index.php?q=" + Uri.EscapeDataString(url);
                while (trial++ < 6)
                {
                    var response = await SocketManager.Pull(url, headers, ExchangeId, counter, Name);

                    if (response.IsSuccess)
                    {
                        try
                        {
                            var r = JsonConvert.DeserializeObject<T>(response.Response);
                            if (r == null)
                            {
                                throw new Exception("Null error");
                            }

                            return r;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Url: " + url + " " + e);
                            if (trial == 6)
                                throw;
                            Thread.Sleep(500);
                        }
                    }

                    //using (var response = await Client.GetAsync(url).ConfigureAwait(false))
                    //{
                    //    var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    //    var content = Encoding.UTF8.GetString(bytes);
                    //    try
                    //    {
                    //        var r = JsonConvert.DeserializeObject<T>(content);
                    //        if (r == null)
                    //        {
                    //            throw new Exception("Null error");
                    //        }

                    //        return r;
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        Console.WriteLine("Url: " + url + " " + e);
                    //        if (trial == 6)
                    //            throw;
                    //        Thread.Sleep(500);
                    //    }
                    //}
                    Console.WriteLine("Retrying...");
                }
                throw new Exception("Null error");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return default(T);
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