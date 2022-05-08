using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSpyMatchmaker
{
    internal class ServerSend : PacketSend
    {
        /// <summary>
        /// Sends a packet to terminate a server
        /// </summary>
        /// <param name="_id">server's id</param>
        public static void TerminateServer(int _id)
        {
            using (Packet packet = new((int)MatchmakerServerPackets.terminationRequest))
            {
                SendTCPData(_id, packet);
            }
        }
    }
}