using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSpyMatchmaker
{
    internal class ClientSend : PacketSend
    {
        /// <summary>
        /// Sends the latest database update to a single client
        /// </summary>
        /// <param name="_id">client's id</param>
        public static void UpdateClient(int _id)
        {
            using Packet packet = new((int)MatchmakerClientPackets.updateReply);
            // Write entry count
            packet.Write(RoomHandler.Entries.Count);

            // Write entries
            for (int i = 0; i < RoomHandler.Entries.Count; i++)
            {
                // Write port
                packet.Write(RoomHandler.Entries[i].Port);
                // Write player count
                packet.Write(RoomHandler.Entries[i].PlayerCount);
                // Write server running state
                packet.Write(RoomHandler.Entries[i].Running);
            }

            SendTCPData(_id, packet, false);
        }

        /// <summary>
        /// Sends the latest database update to all connected clients
        /// </summary>
        public static void UpdateClient()
        {
            using Packet packet = new((int)MatchmakerClientPackets.updateReply);
            // Write entry count
            packet.Write(RoomHandler.Entries.Count);

            // Write entries
            for (int i = 0; i < RoomHandler.Entries.Count; i++)
            {
                // Write port
                packet.Write(RoomHandler.Entries[i].Port);
                // Write player count
                packet.Write(RoomHandler.Entries[i].PlayerCount);
                // Write server running state
                packet.Write(RoomHandler.Entries[i].Running);
            }

            SendTCPDataToAll(packet, false);
        }
    }
}