using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public static int ChildrenCount => Children.Count;
        public static int MaxCount = 30;
        

        public static IApplicationBuilder UseSocketServer(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            return
                app.Use(async (context, next) =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        try
                        {
                            await Process(webSocket);
                        }
                        catch (Exception e)
                        {
                            lock (Children)
                                Children.RemoveAll(c => c.Socket.Equals(webSocket));
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                    else
                    {
                        await next();
                    }

                });
        }

        public static int NextIndex(int borsaId, int currentCounter)
        {
            if (ChildrenByBorsa.ContainsKey(borsaId))
            {
                if (ChildrenByBorsa[borsaId].Count > currentCounter+1)
                    return currentCounter + 1;
                return 0;
            }

            if (Children.Count > currentCounter+1)
                return currentCounter + 1;
            return 0;
        }

        public static void AlignForBorsa(int borsaId, int count)
        {
            lock (Children)
            {
                var children = Children.Take(count).ToList();
                var list = new List<ChildSocket>();
                foreach (var child in children)
                {
                    Children.Remove(child );
                    list.Add(child);
                }
                ChildrenByBorsa[borsaId] = list;
                MaxCount -= count;
            }
        }


        private static async Task Process(WebSocket webSocket)
        {
            //add child
            lock (Children)
            {
                Children.Add(new ChildSocket(webSocket, IdCounter++));
            }
            Console.WriteLine("Child socket registered");

            var buffer = new Byte[8192];
            var segment = new ArraySegment<byte>(buffer);
            int id;

            try
            {
                var content = await ReadSocket(webSocket, segment);
                while (content != null)
                {
                    var req = JsonConvert.DeserializeObject<RequestModel>(content);
                    if (TaskQueue.TryGetValue(req.RequestId, out var task))
                    {
                        task.SetResult(req);
                    }
                    Array.Clear(buffer, 0, buffer.Length);
                    content = await ReadSocket(webSocket, buffer);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                lock (Children)
                    Children.RemoveAll(c => c.Socket.Equals(webSocket));

                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Request could not be processed", CancellationToken.None);
                Console.WriteLine("Child socket removed");
                return;
            }
            
            lock (Children)
                Children.RemoveAll(c => c.Socket.Equals(webSocket));

            await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Closed", CancellationToken.None);
            Console.WriteLine("Child socket removed");
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
        public static async Task<RequestModel> Pull(string url, Tuple<string, string>[] headers, int borsaId, int childId, string borsaName)
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
            if (ChildrenByBorsa.TryGetValue(borsaId, out var list))
                child = list[childId];
            else
                child = Children[childId];
            Console.WriteLine($"{borsaName} pulling on #{child.Id}");
            lock (child.SyncWriteObject)
            {
                child.Socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req))), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
            }

            return await tcs.Task;
        }
    }
}
