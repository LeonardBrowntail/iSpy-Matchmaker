using System;

namespace iSpyMatchmaker
{
    /// <summary>
    /// Various functions to handle incoming messages from unity server
    /// </summary>
    public class ServerHandle
    {
        /// <summary>
        /// Function to read initalization packet
        /// </summary>
        /// <param name="_senderID">server's id</param>
        /// <param name="_packet">packet to be read</param>
        public static void HandleInitReply(int _senderID, Packet _packet)
        {
            try
            {
                // read port
                var newPort = (ushort)_packet.ReadInt();
                // read max player count
                var maxPlayers = _packet.ReadInt();
                // create a new server entry
                var newEntry = new ServerDataEntry(newPort, maxPlayers);

                // add new server entry in database
                RoomHandler.Entries.Add(_senderID, newEntry);
                Console.WriteLine($"Server({_senderID}): handled initialization packet");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when handling Init packet from server-{_senderID}, {e.Message}");
            }
        }

        /// <summary>
        /// Function to read update packet
        /// </summary>
        /// <param name="_senderID">server's id</param>
        /// <param name="_packet">packet to be read</param>
        public static void HandleUpdateReply(int _senderID, Packet _packet)
        {
            // read new player count
            var newPlayerCount = _packet.ReadInt();
            // read new running state
            var newRunning = _packet.ReadBool();

            // update internal database
            if (RoomHandler.Entries[_senderID] == null)
            {
                Console.WriteLine("Warning! Tried to update a non-existent entry!");
                return;
            }
            var temp = new ServerDataEntry(RoomHandler.Entries[_senderID]);
            temp.UpdateEntry(newPlayerCount);
            temp.UpdateEntry(newRunning);
            RoomHandler.Entries[_senderID] = temp;
            Console.WriteLine($"Server({_senderID}): handled update packet");

            // broadcast update to every connected clients
            ClientSend.SendUpdate();
        }

        /// <summary>
        /// Function to read termination confirmation from server
        /// </summary>
        /// <param name="_senderID">server's id</param>
        /// <param name="_packet">packet to be read</param>
        public static void HandleTerminationReply(int _senderID, Packet _packet)
        {
            // terminate rooms
            if (RoomHandler.Entries[_senderID] != null)
            {
                RoomHandler.Singleton.TerminateRoom(RoomHandler.Entries[_senderID].Port);
                RoomHandler.Entries.Remove(_senderID);
            }
            Console.WriteLine($"Server({_senderID}): handled termination packet");
        }
    }
}