using System;
using System.Collections.Concurrent;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ZeegZag.Crawler2.Entity;

namespace ZeegZag.Crawler2.Services.Database
{
    /// <summary>
    /// All items will be saved to database periodically in this service
    /// </summary>
    public static class DatabaseService
    {
        private static DbContextOptions<zeegzagContext> _dbOptions;
        private static Timer _timer;
        private static readonly ConcurrentQueue<IDatabaseJob> JobQueue = new ConcurrentQueue<IDatabaseJob>();

        /// <summary>
        /// Itializes database service with the given interval
        /// </summary>
        /// <param name="config">Config object to get connection string</param>
        /// <param name="interval">Interval in seconds to save items to database</param>
        public static void Initialize(IConfiguration config, int interval)
        {
            var conString = config.GetConnectionString("DefaultConnection");
            _dbOptions = new DbContextOptionsBuilder<zeegzagContext>().UseMySql(conString).Options;


            _timer = new Timer(interval*1000);
            _timer.Elapsed += OnTick;
            _timer.Start();
        }

        /// <summary>
        /// Creates a new database context
        /// </summary>
        /// <returns></returns>
        public static zeegzagContext CreateContext()
        {
            return new zeegzagContext(_dbOptions);
        }

        /// <summary>
        /// Enqueue a database job
        /// </summary>
        /// <param name="job"></param>
        public static void Enqueue(IDatabaseJob job)
        {
            JobQueue.Enqueue(job);
        }
        private static void OnTick(object sender, ElapsedEventArgs e)
        {
            //if (!JobQueue.IsEmpty)
                //Console.WriteLine("Saving...");
            _timer.Stop();
            try
            {
                var  dt= DateTime.Now;
                using (var db = CreateContext())
                {

                    int i = 0;
                    while (JobQueue.TryDequeue(out var job))
                    {
                        job.Execute(db);
                        i++;
                        //Console.Write("..." + job.ToString());
                    }
                    UsdGeneratorJob.ClearCache();
                    if (i > 0)
                    {
                        db.SaveChanges();
                       // Console.WriteLine(string.Format("Executed {0} jobs in {1}s", i, Math.Round((DateTime.Now - dt).TotalSeconds, 2)));
                    }
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            _timer.Start();
        }
    }
}
