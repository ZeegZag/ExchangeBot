using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ZeegZag.Crawler2.Services.Database;

namespace ZeegZag.Crawler2.Services
{
    /// <summary>
    /// It is used in startup to initialize services
    /// </summary>
    public static class ServiceManager
    {
        private static List<ExchangeServiceBase> _allServices;

        private static IEnumerable<Type> GetAllTypesOf<T>()
        {
            var assembly = typeof(ServiceManager).Assembly;
            return assembly.ExportedTypes
                .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract);
        }

        /// <summary>
        /// Initializes all services with a given delay
        /// </summary>
        /// <param name="config">Config object to get connection string</param>
        /// <param name="delay">Delay in milliseconds</param>
        public static void InitalizeServices(IConfiguration config, int delay)
        {
            _allServices = GetAllTypesOf<ExchangeServiceBase>().Select(t => (ExchangeServiceBase)Activator.CreateInstance(t)).ToList();
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(delay);

                DatabaseService.Initialize(config, 20);

                using (var db = DatabaseService.CreateContext())
                {
                    foreach (var service in _allServices)
                        service.Init(db);
                }
            });
        }
    }
}
