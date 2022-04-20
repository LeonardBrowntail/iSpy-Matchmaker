using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSpyMatchmaker
{
    internal class ServerDataEntry
    {
        private ushort port;
        private int playerCount;
        private bool running;

        public int PlayerCount => playerCount;
        public ushort Port => port;
        public bool Running => running;

        public ServerDataEntry(ushort newPort)
        {
            port = newPort;
            playerCount = 0;
            running = false;
        }

        public void UpdateEntry(int newCount)
        {
            playerCount = newCount;
        }

        public void UpdateEntry(bool isRunning)
        {
            running = isRunning;
        }
    }
}