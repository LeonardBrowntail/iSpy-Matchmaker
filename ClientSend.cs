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
        public static void SendInit(int _id)
        {
            using Packet packet = new((int)MatchmakerClientPackets.init);
            // Write entry count
            packet.Write(RoomHandler.Entries.Count);

            // Write entries
            for (int i = 0; i < RoomHandler.Entries.Count; i++)
            {
                // Write port
                packet.Write(RoomHandler.Entries[i].Port);
                // Write max player count
                packet.Write(RoomHandler.Entries[i].MaxPlayer);
            }

            SendTCPData(_id, packet);

            Console.WriteLine($"Client({_id}): sent an init packet");
        }

        /// <summary>
        /// Sends the latest database update to all connected clients
        /// </summary>
        public static void SendUpdate()
        {
            using Packet packet = new((int)MatchmakerClientPackets.update);

            // check how many entries are updated
            int updatedCount = 0;
            foreach (var entry in RoomHandler.Entries)
            {
                if (entry.Value.Updated)
                {
                    updatedCount += 1;
                }
            }

            // write updated count
            packet.Write(updatedCount);

            // check which of all entries are updated
            for (int i = 0; i < RoomHandler.Entries.Count; i++)
            {
                if (RoomHandler.Entries[i].Updated)
                {
                    // write updated entry index
                    packet.Write(i);
                    // Write updated entry player count
                    packet.Write(RoomHandler.Entries[i].PlayerCount);
                    // Write updated entry running state
                    packet.Write(RoomHandler.Entries[i].Running);
                }
            }

            SendTCPDataToAll(packet);

            Console.WriteLine($"Broadcasted update to all connected clients");
        }
    }
}