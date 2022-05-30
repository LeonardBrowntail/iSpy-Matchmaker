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
        /// <summary>
        /// Entry data: server's listening port
        /// </summary>
        public ushort Port { get; private set; }

        /// <summary>
        /// Entry data: server's current player count
        /// </summary>
        public int PlayerCount { get; private set; }

        /// <summary>
        /// Entry data: server's maximum number of connections
        /// </summary>
        public int MaxPlayer { get; private set; }

        /// <summary>
        /// Entry data: server's current running state
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// This parameter is used only when handling an updated data
        /// </summary>
        public bool Updated { get; private set; }

        /// <summary>
        /// Clones an instance of <see cref="ServerDataEntry"/>
        /// </summary>
        /// <param name="clone"></param>
        public ServerDataEntry(ServerDataEntry clone)
        {
            Port = clone.Port;
            PlayerCount = clone.PlayerCount;
            MaxPlayer = clone.MaxPlayer;
            Running = clone.Running;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ServerDataEntry"/>
        /// </summary>
        /// <param name="_newPort">server's listening port</param>
        /// <param name="_maxPlayers">server's max player</param>
        public ServerDataEntry(ushort _newPort, int _maxPlayers)
        {
            Port = _newPort;
            MaxPlayer = _maxPlayers;
            PlayerCount = 0;
            Running = false;
        }

        /// <summary>
        /// Updates the entry's current player count
        /// </summary>
        /// <param name="newCount">new player count</param>
        public void UpdateEntry(int newCount)
        {
            Updated = true;
            PlayerCount = newCount;
        }

        /// <summary>
        /// Updates the entry's current running state
        /// </summary>
        /// <param name="isRunning">new state</param>
        public void UpdateEntry(bool isRunning)
        {
            Updated = true;
            Running = isRunning;
        }

        /// <summary>
        /// Called when the entry has been broadcasted
        /// </summary>
        public void Broadcasted()
        {
            Updated = false;
        }

        /// <summary>
        /// Prints the entry's data to the console
        /// </summary>
        /// <returns>a string of the entry's data</returns>
        public override string ToString()
        {
            string s = $"port={Port}, playerCount={PlayerCount}, MaxPlayer={MaxPlayer}, Runnin={Running}, Updated={Updated}";
            return s;
        }
    }
}