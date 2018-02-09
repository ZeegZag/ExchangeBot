using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ZeegZag.Data.Entity;
using ZeegZag.Data.Models;

namespace ZeegZag.Crawler2.Services.Database
{
    /// <summary>
    /// All items will be saved to database periodically in this service
    /// </summary>
    public static class DatabaseService
    {
        static Queue<BotMessage> MessageQueue = new Queue<BotMessage>();
        private static DbContextOptions<admin_zeegzagContext> _dbOptions;
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
            _dbOptions = new DbContextOptionsBuilder<admin_zeegzagContext>().UseMySql(conString).Options;


            _timer = new Timer(interval*1000);
            _timer.Elapsed += OnTick;
            _timer.Start();
        }

        /// <summary>
        /// Creates a new database context
        /// </summary>
        /// <returns></returns>
        public static admin_zeegzagContext CreateContext()
        {
            return new admin_zeegzagContext(_dbOptions);
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
                        //if (i % 500 == 0)
                        //{
                        //    db.SaveChanges();
                        //}
                    }
                    UsdGeneratorJob.ClearCache();


                    if (i > 0)
                    {
                        db.SaveChanges();
                        // Console.WriteLine(string.Format("Executed {0} jobs in {1}s", i, Math.Round((DateTime.Now - dt).TotalSeconds, 2)));
                    }
                    SendMessagesAsync().Wait();
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            _timer.Start();
        }
        public static void EnqueueBroadcast(long? chatId, string message, IReplyMarkup keyboard = null, ParseMode parseMode = ParseMode.Default)
        {
            if (chatId.HasValue)
                MessageQueue.Enqueue(new BotMessage
                {
                    ChatId = chatId.Value,
                    Message = message,
                    Markup = keyboard,
                    ParseMode = parseMode
                });
        }

        static async Task SendMessagesAsync()
        {
            if (MessageQueue.Count > 0)
            {
                var list = MessageQueue.ToArray();
                MessageQueue.Clear();
                using (var cl = new HttpClient())
                {
                    var r = await cl.PostAsync("http://caster.zeegzag.com/enqueue/alarm",
                        new StringContent(BotMessage.Serialize(list), Encoding.UTF8, "application/json"));

                }
            }
        }
    }
}
