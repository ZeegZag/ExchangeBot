using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services
{
    public class ChildSocket
    {
        public WebSocket Socket { get; }
        public int Id { get; }

        public object SyncWriteObject = new object();

        public ChildSocket(WebSocket socket, int id)
        {
            Socket = socket;
            Id = id;
        }
    }
}
