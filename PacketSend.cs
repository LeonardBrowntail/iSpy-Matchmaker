using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSpyMatchmaker
{
    internal class PacketSend
    {
        protected static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Matchmaker.clients[_toClient].tcp.SendData(_packet);
        }

        protected static void SendTCPDataToAllServers(Packet _packet)
        {
            _packet.WriteLength();
        }
    }
}