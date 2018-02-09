using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            ServicePointManager.DefaultConnectionLimit = 9999999;
            ServicePointManager.MaxServicePoints = 999999;
            ServicePointManager.ReusePort = true;
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.Expect100Continue = false;

            _allServices = GetAllTypesOf<ExchangeServiceBase>().Select(t => (ExchangeServiceBase)Activator.CreateInstance(t)).ToList();
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(delay);

                DatabaseService.Initialize(config, 30);

                using (var db = DatabaseService.CreateContext())
                {
                    foreach (var service in _allServices)
                    {
                        if (!service.IsDisabled)
                        {
//#if DEBUG
//                            if (!Tester.IsServiceTesting(service))
//                                continue;
//#endif

                            if (!SocketManager.Borsa.Borsas.ContainsKey(service.Name))
                                continue;

                            service.Init(db);

                            var count = SocketManager.Borsa.Borsas[service.Name];
                            if (count > 0)
                                SocketManager.AlignForBorsa(service.ExchangeId, SocketManager.Borsa.Borsas[service.Name]);
                            //Thread.Sleep(45000);
                        }
                    }
                }
            });
        }
    }
}
