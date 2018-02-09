using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ZeegZag.Crawler2.Services
{
    public class ChildSocket
    {
        public static Dictionary<WebSocket, ChildSocket> dic = new Dictionary<WebSocket, ChildSocket>();
        public WebSocket Socket { get; }
        public int Id { get; }
        public string Name { get; }
        public List<ChildSocket> Owner { get; set; }
        public bool IsDisposed { get; set; }

        public object SyncWriteObject = new object();
        public bool isLocked = false;

        

        public ChildSocket(WebSocket socket, int id, string name, List<ChildSocket> owner)
        {
            Socket = socket;
            Id = id;
            Name = name;
            Owner = owner;
            dic[Socket] = this;
        }

        public static ChildSocket FindByWebsocket(WebSocket socket)
        {
            if (dic.ContainsKey(socket))
                return dic[socket];
            return null;
        }

        public void Dispose(bool remove)
        {
            if (remove)
            {
                dic.Remove(Socket);
                Owner.Remove(this);
            }
            try
            {
                //Socket?.Dispose();
                IsDisposed = true;
            }
            catch (Exception e)
            {
            }
        }
    }
}
