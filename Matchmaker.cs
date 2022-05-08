using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace iSpyMatchmaker
{
    /// <summary>
    /// Matchmaker server class, handles traffic from both Unity clients and Unity servers
    /// </summary>
    internal class Matchmaker
    {
        private static Matchmaker singleton;
        private bool initialized = false;

        public static Matchmaker Singleton
        { get { if (singleton == null) singleton = new Matchmaker(); return singleton; } }

        private ushort port;
        private int maxClientConnections;
        private int maxServerConnections;
        private bool serversReady = false;
        private TcpListener server;

        public int MaxServer => maxServerConnections;

        /// <summary>
        /// Dictionary to keep track of servers connected
        /// </summary>
        private Dictionary<int, Client> servers = new();

        /// <summary>
        /// Dictionary to keep track of clients connected
        /// </summary>
        private Dictionary<int, Client> clients = new();

        /// <summary>
        /// Get matchmaker's server dictionary
        /// </summary>
        public static Dictionary<int, Client> Servers => singleton.servers;

        /// <summary>
        /// Get matchmaker's client dictionary
        /// </summary>
        public static Dictionary<int, Client> Clients => singleton.clients;

        private Matchmaker()
        {
            maxClientConnections = 0;
            maxServerConnections = 0;
            port = 0;
        }

        public void Initialize(int _serverCount, ushort _port)
        {
            maxClientConnections = 50;
            maxServerConnections = _serverCount;
            port = _port;
            initialized = true;
        }

        /// <summary>
        /// Start method to open matchmaker server for incoming connections
        /// </summary>
        /// <param name="_max">Maximum clients to connect in the matchmaker server</param>
        /// <param name="_port">Port to listen to</param>
        public void Start()
        {
            if (!initialized)
            {
                Console.WriteLine($"Mmatchmaker is not yet initialized");
            }
            // Initialize server data
            Console.WriteLine($"Server starting...");
            InitializeServerData();
            InitializeClientData();

            server = new(IPAddress.Any, port);
            server.Start();

            Console.WriteLine($"Please refrain any connections from outside");
            server.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
            Console.WriteLine($"Server started on {server.Server.RemoteEndPoint}: {port}...");
        }

        /// <summary>
        /// Called when <c>server</c> received a connection from clients
        /// </summary>
        /// <param name="res"></param>
        private void TcpConnectCallback(IAsyncResult res)
        {
            var client = server.EndAcceptTcpClient(res);
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");
            server.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);

            if (!serversReady)
            {
                var remote = (IPEndPoint)client.Client.RemoteEndPoint;
                var local = (IPEndPoint)client.Client.LocalEndPoint;
                if (remote.Address == local.Address)
                {
                    for (int i = 1; i <= maxServerConnections; i++)
                    {
                        if (Servers[i].Transport.socket == null)
                        {
                            Servers[i].Transport.Connect(client);
                            Console.WriteLine($"Server - {i} is connected");
                            foreach (var entry in Servers)
                            {
                                if (entry.Value.Transport.socket == null)
                                {
                                    return;
                                }
                                Console.WriteLine($"All servers are connected, listening to clients...");
                                serversReady = true;
                            }
                            return;
                        }

                        Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect, something is wrong");
                    }
                }
                else
                {
                    Console.WriteLine($"Foreign IP tried to connect whilst listening for local servers...");
                }
            }
            else
            {
                for (int i = 1; i <= maxClientConnections; i++)
                {
                    if (Clients[i].Transport.socket == null)
                    {
                        Clients[i].Transport.Connect(client);
                        return;
                    }

                    Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: server is full!");
                }
            }
        }

        private void InitializeServerData()
        {
            for (int i = 1; i < maxServerConnections; i++)
            {
                Servers.Add(i, new(i, true));
            }
        }

        private void InitializeClientData()
        {
            for (int i = 1; i < maxClientConnections; i++)
            {
                Clients.Add(i, new(i));
            }
        }
    }
}