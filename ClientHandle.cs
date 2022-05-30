using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSpyMatchmaker
{
    /// <summary>
    /// Various functions to handle packets sent by clients
    /// </summary>
    internal class ClientHandle
    {
        /// <summary>
        /// Function to handle update request from a client
        /// </summary>
        /// <param name="_senderID">client's id</param>
        /// <param name="_packet">packet to be read</param>
        public static void HandleUpdate(int _senderID, Packet _packet)
        {
            Console.WriteLine($"Client {_senderID} requested update");
            ClientSend.SendInit(_senderID);
        }

        /// <summary>
        /// Function to handle disconnection request from a client
        /// </summary>
        /// <param name="_senderID">client's id</param>
        /// <param name="_packet">packet to be read</param>
        public static void HandleDisconnect(int _senderID, Packet _packet)
        {
            Console.WriteLine($"Client {_senderID} wants to disconnect");
            Matchmaker.Clients[_senderID].Disconnect();
        }
    }
}