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

        public int id;
        public TCP tcp;

        public Client(int _clientId)
        {
            id = _clientId;
            tcp = new(_clientId);
        }

        /// <summary>
        /// This class handles the TCP connection between matchmaker and client
        /// </summary>
        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private byte[] receiveBuffer;

            private Packet receivedPacket;

            public TCP(int _id)
            {
                id = _id;
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

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
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
                try
                {
                    int _bytelength = stream.EndRead(_result);
                    if (_bytelength <= 0)
                    {
                        // todo: Disconnect
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
                    //todo: Disconnect
                }
            }

            /// <summary>
            /// Send data to the client using TCP
            /// </summary>
            /// <param name="dataStream">packet to be sent</param>
            public void SendData(Packet dataStream)
            {
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
        }
    }
}