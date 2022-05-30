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
        /// <summary>
        /// The one and only matchmaker
        /// </summary>
        private static Matchmaker singleton;

        /// <summary>
        /// The one and only matchmaker
        /// </summary>
        public static Matchmaker Singleton
        { get { if (singleton == null) singleton = new Matchmaker(); return singleton; } }

        /// <summary>
        /// Is the matchmaker initialized
        /// </summary>
        private bool initialized = false;

        /// <summary>
        /// Matchmaker's listening port
        /// </summary>
        private ushort port;

        /// <summary>
        /// The number of maximum client connection
        /// </summary>
        private int maxClientConnections;

        /// <summary>
        /// The number of maximum server connections
        /// </summary>
        private int maxServerConnections;

        /// <summary>
        ///
        /// </summary>
        private bool serversReady = false;

        private TcpListener matchmakerServer;

        public int Port => port;

        /// <summary>
        /// Dictionary to keep track of Unity servers connected
        /// </summary>
        private Dictionary<int, Client> servers = new();

        /// <summary>
        /// Dictionary to keep track of Unity clients connected
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
            Console.WriteLine($"Matchmaker initialized, serverCount = {maxServerConnections}, port = {port}");
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
                return;
            }

            // Initialize server data
            Console.WriteLine($"Server starting...");
            InitializeServerData();
            InitializeClientData();

            matchmakerServer = new(IPAddress.Any, port);
            matchmakerServer.Start();

            Console.WriteLine($"Server started on {matchmakerServer.Server.LocalEndPoint}...");
            Console.WriteLine($"Please hold any connections from outside except from Unity Servers...");
            matchmakerServer.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
        }

        /// <summary>
        /// Called when <c>server</c> received a connection from clients
        /// </summary>
        /// <param name="res"></param>
        private void TcpConnectCallback(IAsyncResult res)
        {
            try
            {
                var client = matchmakerServer.EndAcceptTcpClient(res);
                Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");
                matchmakerServer.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);

                // opens connections for servers
                if (!serversReady)
                {
                    for (int i = 1; i <= maxServerConnections; i++)
                    {
                        if (Servers[i].Transport.socket == null)
                        {
                            Servers[i].Transport.Connect(client);
                            Console.WriteLine($"Server - {i} is registered");
                            break;
                        }
                    }
                    // do a check whether the server connection slots are exhausted, if yes, then the server connections are ready.
                    serversReady = true;
                    foreach (var item in Servers)
                    {
                        if (!item.Value.TransportInitialized) serversReady = false;
                    }
                    if (serversReady) Console.WriteLine("All servers has been registered");
                }
                else
                {
                    // after the server connections are ready, opens connections for clients
                    for (int i = 1; i <= maxClientConnections; i++)
                    {
                        if (Clients[i].Transport.socket == null)
                        {
                            Clients[i].Transport.Connect(client);
                            ClientSend.SendInit(i);
                            Console.WriteLine($"Client - {i} is registered");
                            break;
                        }
                        Console.WriteLine($"Client {client.Client.RemoteEndPoint} failed to connect: server is full!");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Matchmaker: {e.Message}");
            }
        }

        /// <summary>
        /// Initialize a list of connections to Unity servers and their ids
        /// </summary>
        private void InitializeServerData()
        {
            for (int i = 1; i <= maxServerConnections; i++)
            {
                Servers.Add(i, new(i, true));
            }
            Console.WriteLine($"Server connection list initialized\n----------------------------------");
        }

        /// <summary>
        /// Initialize a list of connections to Unity clients and their ids
        /// </summary>
        private void InitializeClientData()
        {
            for (int i = 1; i <= maxClientConnections; i++)
            {
                Clients.Add(i, new(i));
            }
            Console.WriteLine($"Client connection list initialized\n----------------------------------");
        }

        /// <summary>
        /// Stops the matchmaker from listening and discards all connections
        /// </summary>
        public void Stop()
        {
            foreach (var item in clients)
            {
                if (item.Value.Transport.socket != null)
                {
                    item.Value.Disconnect();
                }
            }
            foreach (var item in servers)
            {
                if (item.Value.Transport.socket != null)
                {
                    item.Value.Disconnect();
                }
            }
            clients = null;
            servers = null;
            matchmakerServer.Stop();
            Console.WriteLine($"Matchmaker: stopped everything");
        }
    }
}