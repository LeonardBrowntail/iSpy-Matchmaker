using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSpyMatchmaker
{
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
            ClientSend.UpdateClient(_senderID);
        }
    }
}