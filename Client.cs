using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace iSpyMatchmaker
{
    internal class Client
    {
        public static int dataBufferSize = 4096;

        /// <summary>
        /// Client's or Server's internal matchmaking ID
        /// </summary>
        /// <remarks>
        /// Used in packet sending.
        /// Unique for every connection, whether it is a server or a client.
        /// Very important that no two connections to have the same id, otherwise packet sending would be a total mess.
        /// However, a client and a server may have the same ID.
        /// To differenciate between a client and a server, <c>isServer</c> bool is used.
        /// </remarks>
        private readonly int id;

        private bool isServer;
        private TCP tcp;

        /// <summary>
        /// Client's or Server's internal matchmaking ID
        /// </summary>
        /// <remarks>
        /// Used in packet sending.
        /// Unique for every connection, whether it is a server or a client.
        /// Very important that no two connections to have the same id, otherwise packet sending would be a total mess.
        /// However, a client and a server may have the same ID.
        /// To differenciate between a client and a server, <c>isServer</c> bool is used.
        /// </remarks>
        public int ID => id;

        public bool IsServer => isServer;
        public TCP Transport => tcp;

        /// <summary>
        /// Creates a new client connection between matchmaker and client or server
        /// </summary>
        /// <param name="_clientId">id to be assigned</param>
        /// <param name="_isServer"><c>true</c>, if this connection is with a Unity server</param>
        public Client(int _clientId, bool _isServer = false)
        {
            id = _clientId;
            isServer = _isServer;
            tcp = new(_clientId, _isServer);
            Console.WriteLine($"Created a new {(isServer ? "server" : "client")} connection (id = {id})");
        }

        /// <summary>
        /// This class handles the TCP connection between matchmaker and client
        /// </summary>
        public class TCP
        {
            public TcpClient socket;

            // Tags
            /// <summary>
            /// This TCP's id, which is the same as its client
            /// </summary>
            private readonly int id;

            /// <summary>
            /// This TCP's server tag, the same as client
            /// </summary>
            private bool isServer;

            // Packet handling
            private NetworkStream stream;

            private byte[] receiveBuffer;
            private Packet receivedPacket = null;

            private delegate void PacketHandler(int _senderID, Packet _packet);

            // Server handling
            private static Dictionary<int, PacketHandler> serverPacketsHandler = null;

            // Client handling
            private static Dictionary<int, PacketHandler> clientPacketsHandler = null;

            public TCP(int _id, bool _isServer)
            {
                id = _id;
                isServer = _isServer;
                InitializeData(_isServer);
            }

            /// <summary>
            /// Connect to client
            /// </summary>
            /// <remarks>
            /// This function acts as a connection opener for the matchmaker to the client.
            /// Once the matchmaker is connected to the client, it will begin to read any incoming data from the matchmaker.
            /// If data is received, <c>ReceiveCallback</c> will be called.
            /// </remarks>
            /// <param name="_socket">Socket to connect to</param>
            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receiveBuffer = new byte[dataBufferSize];

                Console.WriteLine($"Listening for packets from {(isServer ? "server" : "client")}({id})...");
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }

            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedPacket = null;
                receiveBuffer = null;
                socket = null;
            }

            /// <summary>
            /// This function is called when the client's <c>stream</c> has received a stream
            /// </summary>
            /// <remarks>
            /// If a stream is received by the client, it will try to copy the stream into <c>receiveBuffer</c>.
            /// After copying, the buffer will run through <c>HandleData</c> check to see if the packet is whole or segmented.
            /// </remarks>
            private void ReceiveCallback(IAsyncResult _result)
            {
                Console.WriteLine($"Received a packet from {(isServer ? "server" : "client")}({id})!");
                try
                {
                    int _bytelength = stream.EndRead(_result);
                    if (_bytelength <= 0)
                    {
                        if (isServer)
                        {
                            Matchmaker.Servers[id].Disconnect();
                        }
                        else
                        {
                            Matchmaker.Clients[id].Disconnect();
                        }
                        return;
                    }

                    byte[] _data = new byte[_bytelength];
                    Array.Copy(receiveBuffer, _data, _bytelength);

                    receivedPacket.Reset(HandleData(_data));

                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception thrown: {e}");
                    if (isServer)
                    {
                        Matchmaker.Servers[id].Disconnect();
                    }
                    else
                    {
                        Matchmaker.Clients[id].Disconnect();
                    }
                }
            }

            /// <summary>
            /// Send data to the client using TCP
            /// </summary>
            /// <param name="dataStream">packet to be sent</param>
            public void SendData(Packet dataStream)
            {
                Console.WriteLine($"{(isServer ? "server" : "client")}({id}): sending a TCP packet!");
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(dataStream.ToArray(), 0, dataStream.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error sending data to matchmaker: {e}");
                }
            }

            /// <summary>
            /// This function handles incoming data from client.
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
                        using Packet _packet = new(packetBytes);
                        int _packetID = _packet.ReadInt();
                        if (isServer)
                        {
                            serverPacketsHandler[_packetID](id, _packet);
                        }
                        else
                        {
                            clientPacketsHandler[_packetID](id, _packet);
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

            /// <summary>
            /// Initializes packet handlers
            /// </summary>
            /// <param name="_isServer"></param>
            private void InitializeData(bool _isServer)
            {
                if (_isServer)
                {
                    clientPacketsHandler = null;
                    serverPacketsHandler = new()
                    {
                        { (int)ServerMatchmakerPackets.initialization, ServerHandle.HandleInitReply },
                        { (int)ServerMatchmakerPackets.updateReply, ServerHandle.HandleUpdateReply },
                        { (int)ServerMatchmakerPackets.terminationReply, ServerHandle.HandleTerminationReply }
                    };
                    Console.WriteLine($"Initialized packet handlers for server-{id}");
                }
                else
                {
                    serverPacketsHandler = null;
                    clientPacketsHandler = new()
                    {
                        { (int)ClientMatchmakerPackets.updateRequest, ClientHandle.HandleUpdate }
                    };
                    Console.WriteLine($"Initialized packet handlers for client-{id}");
                }
            }
        }

        public void Disconnect()
        {
            Console.WriteLine($"{(isServer ? "server" : "client")}({id}): ({tcp.socket.Client.RemoteEndPoint}) has disconnected...");

            tcp.Disconnect();
        }
    }
}