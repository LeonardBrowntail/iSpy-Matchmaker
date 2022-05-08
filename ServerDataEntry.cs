using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSpyMatchmaker
{
    /// <summary>
    /// Class to contain a server data entry
    /// </summary>
    internal class ServerDataEntry
    {
        private ushort port;
        private int playerCount;
        private int maxPlayer;
        private bool running;

        /// <summary>
        /// Server's port
        /// </summary>
        public ushort Port => port;

        /// <summary>
        /// Server's current player count
        /// </summary>
        public int PlayerCount => playerCount;

        /// <summary>
        /// Server's max player count
        /// </summary>
        public int MaxPlayer => maxPlayer;

        /// <summary>
        /// Server's current running state
        /// </summary>
        public bool Running => running;

        public ServerDataEntry(ServerDataEntry clone)
        {
            port = clone.Port;
            playerCount = clone.PlayerCount;
            maxPlayer = clone.MaxPlayer;
            running = clone.Running;
        }

        public ServerDataEntry(ushort newPort, int maxPlayers)
        {
            port = newPort;
            playerCount = 0;
            running = false;
        }

        /// <summary>
        /// Updates the entry's current player count
        /// </summary>
        /// <param name="newCount">new player count</param>
        public void UpdateEntry(int newCount)
        {
            playerCount = newCount;
        }

        /// <summary>
        /// Updates the entry's current running state
        /// </summary>
        /// <param name="isRunning">new state</param>
        public void UpdateEntry(bool isRunning)
        {
            running = isRunning;
        }
    }
}