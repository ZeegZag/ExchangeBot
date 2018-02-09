using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Core.Sockets;

namespace ZeegZag.Crawler2.Services
{
    public class PullerSession : IDisposable
    {
        public List<CoreSocketClient> Clients = new List<CoreSocketClient>();
        object Locker = new object();
        private int Index = 0;
        public static PullerSession Create()
        {
            PullerSession session = new PullerSession();
            foreach (var socket in SocketManager.Borsa.Sockets)
            {
                try
                {
                    CoreSocketClient client = new CoreSocketClient()
                    {
                        GetTimeout = 30000
                    };
                    client.ConnectAndListen("wss://" + socket + "/ws");
                    session.Clients.Add(client);
                }
                catch (Exception e)
                {
                }
            }

            return session;
        }
        

        public CoreSocketClient GetNextClient()
        {
            lock (Locker)
            {
                Index = Clients.Count > Index + 1 ? Index + 1 : 0;
            }
            return Clients[Index];
        }

        public void Dispose()
        {
            if (Clients != null)
            {
                foreach (var client in Clients)
                    client.Close();
                Clients.Clear();
                Clients = null;

            }
        }
    }
}
