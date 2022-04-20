using System;

namespace iSpyMatchmaker
{
    /// <summary>
    /// Various functions to handle incoming messages from unity server
    /// </summary>
    public class ServerHandle
    {
        public static void HandleInitReply(int _senderID, Packet _packet)
        {
            // Todo: Handle initialization by Unity server
        }

        public static void HandleUpdateReply(int _senderID, Packet _packet)
        {
            // Todo: Handle update reply by Unity server
        }

        public static void HandleTerminationReply(int _senderID, Packet _packet)
        {
            // Todo: Handle termination reply by Unity server
        }
    }
}