using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace iSpyMatchmaker
{
    internal class Server
    {
        private ushort port;
        private int maxPlayer;
        private TcpListener server;
        public static Dictionary<int, Client> clients = new();

        public void Start(int _max, ushort _port)
        {
            maxPlayer = _max;
            port = _port;
            InitializeServerData();
            Console.WriteLine($"Server starting...");
            server = new(IPAddress.Any, port);
            server.Start();
            server.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
            Console.WriteLine($"Server started on {server.Server.RemoteEndPoint}:{port}...");
        }

        private void TcpConnectCallback(IAsyncResult res)
        {
            var client = server.EndAcceptTcpClient(res);
            server.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= maxPlayer; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    return;
                }

                Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: server is full!");
            }
        }

        private void InitializeServerData()
        {
            for (int i = 1; i < maxPlayer; i++)
            {
                clients.Add(i, new Client(i));
            }
        }
    }
}