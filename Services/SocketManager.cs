using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Core.Core.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZeegZag.Crawler2.Services
{
    public static class SocketManager
    {
        static ConcurrentDictionary<string, TaskCompletionSource<RequestModel>> TaskQueue = new ConcurrentDictionary<string, TaskCompletionSource<RequestModel>>();
        static List<ChildSocket> Children = new List<ChildSocket>();
        static ConcurrentDictionary<int, List<ChildSocket>> ChildrenByBorsa = new ConcurrentDictionary<int, List<ChildSocket>>();
        private static int IdCounter = 1;
        public static BorsaObject Borsa;
        static object Locker = new object();

        public static IApplicationBuilder UseSocketClient(this IApplicationBuilder app)
        {
            CoreSocketClient client = new CoreSocketClient();
            client.Reconnected += (sender, args) =>
            {
                sender.Get<bool>("register", Borsa.Id);
            };
            client.ConnectAndListen("wss://zeegzagcrawlermaster.eu-gb.mybluemix.net/ws");
            client.Get<bool>("register-new");
            Borsa = client.Get<BorsaObject>("get");
            //using (var cl = new HttpClient())
            //using (var msg = cl.GetAsync("https://zeegzagcrawlermaster.eu-gb.mybluemix.net/borsa/get").Result)
            //using (var content = msg.Content)
            //{
            //    var res = content.ReadAsStringAsync().Result;
            //    Borsa = JsonConvert.DeserializeObject<BorsaObject>(res);
            //}
            //ConnectAll(true); 
            //System.Timers.Timer t = new System.Timers.Timer(60000 * 60); //reconnect every hour
            //t.Elapsed += (s, args) =>
            //{
            //    Reconnect();
            //};
            //t.Start();
            return app;
        }

        public static void Reconnect()
        {
            MaintenenceEvent.Reset();
            Maintenence = true;
            Console.WriteLine("Maintenance wait mode...");
            Thread.Sleep(60000 * 2);
            Console.WriteLine("Reconnecting...");
            ConnectAll(false);
            Thread.Sleep(60000);
            Console.WriteLine("Maintenance completed");
            Maintenence = false;
            MaintenenceEvent.Set();
        }
        public static int NextIndex(int borsaId, int currentCounter)
        {
            lock (Locker)
            {
                if (ChildrenByBorsa.ContainsKey(borsaId))
                {
                    if (ChildrenByBorsa[borsaId].Count > currentCounter + 1)
                        return currentCounter + 1;
                    return 0;
                }

                if (Children.Count > currentCounter + 1)
                    return currentCounter + 1;
                return 0;
            }
        }

        public static void AlignForBorsa(int borsaId, int count)
        {
            lock (Locker)
            {
                var children = Children.Take(count).ToList();
                var list = new List<ChildSocket>();
                foreach (var child in children)
                {
                    Children.Remove(child );
                    list.Add(child);
                }
                ChildrenByBorsa[borsaId] = list;
            }
        }

        public static string StatusText = "";
        public static List<string> FailedConnections = new List<string>();
        public static bool Maintenence { get; set; }
        public static ManualResetEvent MaintenenceEvent = new ManualResetEvent(true);
        private static bool isLocked=false;
        public static void ConnectAll(bool first)
        {
            try
            {
                StatusText = "Connecting...";
                Console.WriteLine("Connecting...");
                FailedConnections.Clear();
                var allSocketIps = Borsa.Sockets.ToList();
                lock (Locker)
                {
                    isLocked = true;
                    StatusText = "Started connecting...";
                    //dc all children
                    int j = 1;
                    foreach (var child in Children.ToList())
                    {
                        //Children.Remove(child);
                        StatusText = "Disconnecting old sockets... " + j++;
                        try
                        {
                            child.Socket.CloseAsync(WebSocketCloseStatus.Empty, String.Empty, CancellationToken.None).Wait();
                        }
                        catch (Exception e)
                        {
                        }
                        //child.Dispose(true);
                    }
                    //dc aligned sockets

                    foreach (var kvp in ChildrenByBorsa.ToArray())
                    foreach (var child in kvp.Value)
                    {
                        try
                        {
                            child.Socket.CloseAsync(WebSocketCloseStatus.Empty, String.Empty, CancellationToken.None).Wait();
                        }
                        catch (Exception e)
                        {
                        }
                        //child.Dispose(false);
                    }
                    if (first)
                    {
                        //connect all children again
                        for (var i = 0; i < allSocketIps.Count; i++)
                        {
                            StatusText = $"Connecting... ({i + 1}/{allSocketIps.Count})";
                            var socketIp = allSocketIps[i];
                            ClientWebSocket socket = new ClientWebSocket();
                            try
                            {
                                socket.ConnectAsync(new Uri("wss://" + socketIp + "/ws"), CancellationToken.None)
                                    .Wait();
                                var childSocket = new ChildSocket(socket, IdCounter++, socketIp, Children);
                                Children.Add(childSocket);
                                KeepListening(childSocket);
                            }
                            catch (Exception e)
                            {
                                FailedConnections.Add(socketIp);
                            }
                        }
                    }
                    /*
                    //align sockets again
                    foreach (var kvp in ChildrenByBorsa.ToArray())
                    {
                        var oldList = kvp.Value.ToList();
                        //align new sockets
                        var listOfAlignedChildren = Children.Take(oldList.Count).ToList();
                        var newList = new List<ChildSocket>();
                        foreach (var child in listOfAlignedChildren)
                        {
                            Children.Remove(child);
                            newList.Add(child);
                            child.Owner = newList;
                        }

                        ChildrenByBorsa[kvp.Key] = newList;

                        //dc old aligned sockets
                        foreach (var child in oldList)
                        {
                            try
                            {
                                //child.Socket.CloseAsync(WebSocketCloseStatus.Empty, String.Empty, CancellationToken.None);
                            }
                            catch (Exception e)
                            {
                            }
                            child.Dispose(true);
                        }
                    }
                    */
                }

                isLocked = false;

                StatusText = $"Connected to {allSocketIps.Count - FailedConnections.Count} sockets";
                Console.WriteLine("Connected");
            }
            catch (Exception e)
            {
                StatusText = e.ToString();
            }
        }

        private static async Task KeepListening(ChildSocket child)
        {
            var webSocket = child.Socket;
            var buffer = new Byte[8192];
            var segment = new ArraySegment<byte>(buffer);

            try
            {
                var content = await ReadSocket(webSocket, segment);
                while (content != null)
                {
                    var req = JsonConvert.DeserializeObject<RequestModel>(content);
                    if (TaskQueue.TryRemove(req.RequestId, out var task))
                    {
                        task.SetResult(req);
                    }
                    Array.Clear(buffer, 0, buffer.Length);
                    content = await ReadSocket(webSocket, buffer);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            child.IsDisposed = true;
            //try to reconnect
            if (isLocked)
            {
                if (child.isLocked)
                {
                    TryReconnect(child, webSocket);
                }
                else
                {
                    lock (child.SyncWriteObject)
                    {
                        TryReconnect(child, webSocket);
                    }
                }
            }
            else
            {
                if (child.isLocked)
                {
                    lock (Locker)
                    {
                        TryReconnect(child, webSocket);
                    }
                }
                else
                {
                    lock (Locker)
                    lock (child.SyncWriteObject)
                    {
                        TryReconnect(child, webSocket);
                    }
                }
            }


            try
            {
                //await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Request could not be processed", CancellationToken.None);
            }
            catch (Exception exception)
            {
            }
            //lock (Locker)
                //ChildSocket.FindByWebsocket(webSocket)?.Dispose(false);
            
        }

        private static void TryReconnect(ChildSocket child, WebSocket webSocket)
        {
            try
            {
                try
                {
                    webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Request could not be processed",
                        CancellationToken.None).Wait(3000);
                }
                catch (Exception exception)
                {
                }
                ClientWebSocket socket = new ClientWebSocket();
                socket.ConnectAsync(new Uri("wss://" + child.Name + "/ws"), CancellationToken.None).Wait();
                var childSocket = new ChildSocket(socket, IdCounter++, child.Name, child.Owner);
                child.Owner.Add(childSocket);
                child.Dispose(true);
                KeepListening(childSocket);
                Console.WriteLine("Reconnected");
            }
            catch (Exception e)
            {
                Console.WriteLine("Reconnect failed: " + e.Message);
            }
        }

        static async Task<string> ReadSocket(WebSocket webSocket, ArraySegment<Byte> buffer)
        {
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.CloseStatus.HasValue && !result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
                return null;
            }
        }

        public static long ReqCount = 0;

        public static RequestModel Pull(string url, Tuple<string, string>[] headers, int borsaId,
            int childId, string borsaName)
        {
            var tcs = new TaskCompletionSource<RequestModel>();


            var req = new RequestModel()
            {
                RequestId = Guid.NewGuid().ToString(),
                Url = url,
                Headers = headers.ToList()
            };
            TaskQueue.TryAdd(req.RequestId, tcs);


            ChildSocket child;
            lock (Locker)
            {
                ReqCount++;
                if (ChildrenByBorsa.TryGetValue(borsaId, out var list))
                    child = list[childId];
                else
                    child = Children[childId];
            }
            //Console.WriteLine($"{borsaName} pulling on #{child.Id}");
            lock (child.SyncWriteObject)
            {
                child.isLocked = true;
                if (!child.IsDisposed)
                    child.Socket
                        .SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req))),
                            WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                child.isLocked = false;
            }
            tcs.Task.Wait(10000);
            if (!tcs.Task.IsCompleted)
            {
                TaskQueue.TryRemove(req.RequestId, out var _);
                req.IsSuccess = false;
                return req;
            }
            return tcs.Task.Result;
        }
    }
    public class BorsaObject
    {
        public int Id { get; set; }
        public Dictionary<string, int> Borsas { get; set; }
        public List<string> Sockets { get; set; }
    }
}
