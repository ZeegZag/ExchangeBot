using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services
{
    public static class RateLimitService
    {
        static Dictionary<int, RateLimit> Store = new Dictionary<int, RateLimit>();

        public static void WaitForLimit(int exchangeId)
        {
            bool isRegistered;
            RateLimit limit=null;
            
            lock (Store)
            {
                isRegistered = Store.ContainsKey(exchangeId);
                if (isRegistered)
                    limit = Store[exchangeId];
            }
            //if rate limit registered
            if (isRegistered)
            {
                lock (limit.Locker)
                {

                    bool pass = true;
                    do
                    {
                        var now = DateTime.Now;
                        //var ts = (int) now.TimeOfDay.TotalSeconds;
                        var millis = (long) now.TimeOfDay.TotalMilliseconds;
                        //if in same second
                        //if (limit.SecondsTimestamp == ts)
                        // {
                        //if still not reached limit
                        //if (limit.PassCountPerSecond < limit.Limit)
                        //{
                        var millisPast = millis - limit.MillisTimestamp;
                        if (millisPast < limit.WaitMillis && millisPast >= 0)
                        {
                            //too fast request in a second
                            var millisToWait = limit.WaitMillis - millisPast;
                            if (millisToWait > 0)
                                Thread.Sleep((int) millisToWait);
                            pass = false;
                        }
                        else
                        {
                            limit.PassCountPerSecond++;
                            limit.MillisTimestamp = millis;
                            pass = true;
                        }
                        //}
                        //else
                        //{
                        //    //wait until next second

                        //    var timeToNextSecond =
                        //        (int) (new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second)
                        //                   .AddSeconds(1) - now).TotalMilliseconds;
                        //    Thread.Sleep(timeToNextSecond);
                        //    pass = false;
                        //}
                        //}
                        //else
                        //{
                        //    limit.SecondsTimestamp = ts;
                        //    limit.PassCountPerSecond = 1;
                        //    limit.MillisTimestamp = millis;
                        //   pass = true;
                        //}
                    } while (!pass);
                }
            }
            

        }

        public static void RegisterLimit(int exchangeId, int limitPerSecond, int? millisecondsBetween = null)
        {
            lock (Store)
            {
                Store[exchangeId] = new RateLimit(){PassCountPerSecond = 0, Limit = limitPerSecond, SecondsTimestamp = 0, WaitMillis = millisecondsBetween ?? (1000 / limitPerSecond), MillisTimestamp = 0};
            }
        }
    }

    public class RateLimit
    {
        public int SecondsTimestamp { get; set; }
        public int Limit { get; set; }
        public int PassCountPerSecond { get; set; }
        public long MillisTimestamp { get; set; }
        public int WaitMillis { get; set; }
    }
}
