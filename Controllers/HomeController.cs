using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZeegZag.Crawler2.Services;

namespace ZeegZag.Crawler2.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Ok("Bot is running...");
        }
        public IActionResult Test()
        {
            var list = new List<string>();
            string status;
            using (var session = PullerSession.Create())
            {
                status = "Client count: " + session.Clients.Count;
                foreach (var cl in session.Clients)
                {
                    try
                    {
                        var version = cl.Get<string>("version");
                        if (version != "v3")
                        {
                            list.Add(cl.Url);
                        }
                    }
                    catch (Exception exception)
                    {
                        list.Add(cl.Url);
                    }
                }
            }
            string failedInfo = " - No failed connections.";
            if (list.Count > 0)
                failedInfo = " - Failed connections: " + string.Join(", ", list);
            var reqCount = " - Request count: " + SocketManager.ReqCount;
            return Ok("Bot is running... " + status + failedInfo + reqCount);
        }

        public IActionResult Reconnect()
        {
            System.Timers.Timer t = new System.Timers.Timer(100); //reconnect every hour
            t.Elapsed += (s, args) =>
            {
                t.Stop();
                t.Dispose();
                SocketManager.Reconnect();
            };
            t.Start();
            return Ok("Reconnection completed");
        }
    }
}
