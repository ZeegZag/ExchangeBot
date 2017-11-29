using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services
{
    public static class CachingService
    {
        static ConcurrentDictionary<string, int> Cache = new ConcurrentDictionary<string, int>();

        public static int? CurrencyIdByName(string name, Func<int?> factory)
        {
            return Get("c" + name, factory);
        }
        public static int? Get(string name, Func<int?> factory)
        {
            if (Cache.TryGetValue(name, out int value))
                return value;
            else
            {
                var id = factory.Invoke();
                if (id.HasValue)
                    Cache[name] = id.Value;
                return id;
            }
        }
    }
}
