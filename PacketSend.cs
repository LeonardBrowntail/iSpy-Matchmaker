using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSpyMatchmaker
{
    internal class PacketSend
    {
        /// <summary>
        /// Send a TCP packet to a client
        /// </summary>
        /// <param name="_receiverID">receiver's id</param>
        /// <param name="_packet">packet to be sent</param>
        /// <param name="server"><c>false</c>, for client</param>
        protected static void SendTCPData(int _receiverID, Packet _packet, bool server = false)
        {
            _packet.WriteLength();
            if (server)
            {
                Matchmaker.Servers[_receiverID].Transport.SendData(_packet);
            }
            else
            {
                Matchmaker.Clients[_receiverID].Transport.SendData(_packet);
            }
            Console.WriteLine($"Data sent to {(server ? "server" : "client")}-{_receiverID}");
            Console.WriteLine($"Data sent size: {_packet.Length()}");
        }

        /// <summary>
        /// Sends a TCP packet to all connected clients or servers
        /// </summary>
        /// <param name="_packet">packet to send</param>
        /// <param name="server"><c>false</c>, for clients</param>
        protected static void SendTCPDataToAll(Packet _packet, bool server = false)
        {
            _packet.WriteLength();
            if (server)
            {
                foreach (var s in Matchmaker.Servers)
                {
                    s.Value.Transport.SendData(_packet);
                }
            }
            else
            {
                foreach (var c in Matchmaker.Clients)
                {
                    c.Value.Transport.SendData(_packet);
                }
            }
            Console.WriteLine($"Data sent to {(server ? "servers" : "clients")}");
            Console.WriteLine($"Data sent size: {_packet.Length()}");
        }

        /// <summary>
        /// Sends a TCP packet to all connected clients or servers except one
        /// </summary>
        /// <param name="_receiverID">id to be ignored</param>
        /// <param name="_packet">packet to be sent</param>
        /// <param name="server"><c>false</c>, for clients</param>
        protected static void SendTCPDataExcept(int _receiverID, Packet _packet, bool server = false)
        {
            _packet.WriteLength();
            if (server)
            {
                foreach (var s in Matchmaker.Servers)
                {
                    if (s.Key != _receiverID)
                    {
                        s.Value.Transport.SendData(_packet);
                    }
                }
            }
            else
            {
                foreach (var c in Matchmaker.Clients)
                {
                    if (c.Key != _receiverID)
                    {
                        c.Value.Transport.SendData(_packet);
                    }
                }
            }
            Console.WriteLine($"Data sent to {(server ? "servers" : "clients")} except {_receiverID}");
            Console.WriteLine($"Data sent size: {_packet.Length()}");
        }
    }
}