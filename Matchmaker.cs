using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace iSpyMatchmaker
{
    /// <summary>
    /// Matchmaker server class, handles traffic from both Unity clients and Unity servers
    /// </summary>
    internal class Matchmaker
    {
        private ushort port;
        private int maxPlayer;
        private TcpListener server;
        public static Dictionary<int, Client> servers = new();
        public static Dictionary<int, Client> clients = new();

        // Packet handling
        private Packet receivedPacket;

        private delegate void PacketHandler(Packet _packet);

        private static Dictionary<int, PacketHandler> serverPacketsHandler;
        private static Dictionary<int, PacketHandler> clientPacketsHandler;

        /// <summary>
        /// Start method to open matchmaker server for incoming connections
        /// </summary>
        /// <param name="_max">Maximum clients to connect in the matchmaker server</param>
        /// <param name="_port">Port to listen to</param>
        public void Start(int _max, ushort _port)
        {
            // Set variables
            maxPlayer = _max;
            port = _port;

            // Initialize server data
            Console.WriteLine($"Server starting...");
            InitializeServerData();
            receivedPacket = new();

            server = new(IPAddress.Any, port);
            server.Start();

            server.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
            Console.WriteLine($"Server started on {server.Server.RemoteEndPoint}:{port}...");
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

        /// <summary>
        /// This function handles incoming data from matchmaker.
        /// </summary>
        /// <remarks>
        /// Basically, when a packet is received, it has a chance of it not being whole and, if not handled correctly, may result in data loss.
        /// This may happen because once a packet is used, the content will get discarded for the next incoming packet.
        /// To avoid this, we have to check if the unread length is less than the packet length contained.
        /// If the length contained is different from the data unread, then the data is segmented and some information has yet to come.
        /// </remarks>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            receivedPacket.SetBytes(data);
            if (receivedPacket.UnreadLength() >= 4)
            {
                packetLength = receivedPacket.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 & packetLength <= receivedPacket.UnreadLength())
            {
                byte[] packetBytes = receivedPacket.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(packetBytes))
                    {
                        int _packetID = _packet.ReadInt();
                    }
                });

                packetLength = 0;
                if (receivedPacket.UnreadLength() >= 4)
                {
                    packetLength = receivedPacket.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }
            if (packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        private void InitializeServerData()
        {
            for (int i = 1; i < maxPlayer; i++)
            {
                clients.Add(i, new Client(i));
            }

            serverPacketsHandler = new()
            {
                { (int)ServerMatchmakerPackets.initialization, ServerHandle.HandleInitReply },
                { (int)ServerMatchmakerPackets.updateReply, ServerHandle.HandleUpdateReply },
                { (int)ServerMatchmakerPackets.terminationReply, ServerHandle.HandleTerminationReply }
            };

            clientPacketsHandler = new()
            {
                { (int)ClientMatchmakerPackets.updateRequest, ClientHandle.ClientHandleUpdate }
            }
        }
    }
}