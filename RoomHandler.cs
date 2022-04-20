using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace iSpyMatchmaker
{
    /// <summary>
    /// This class handles the "rooms" opened by the matchmaker.
    /// </summary>
    /// <remarks>
    /// <c>RoomHandler</c> is responsible in the creation and termination of rooms.
    /// <c>RoomHandler</c> is a singleton, therefore there should not be more than one <c>RoomHandler</c> instance.
    /// Rooms are basically Unity game servers that are running individually and listens in separate ports.
    /// </remarks>
    internal class RoomHandler
    {
        // Instance variable
        public static RoomHandler instance;

        /// <summary>
        /// Matchmaker current directory
        /// </summary>
        private readonly string filePath = Directory.GetCurrentDirectory();

        /// <summary>
        /// 1st room port number is set to 1 + matchmaker's port
        /// </summary>
        private readonly ushort roomStartingPort = 27016;

        /// <summary>
        /// Room program name, a.k.a the Unity server program
        /// </summary>
        private string roomName;

        /// <summary>
        /// The number of rooms that are started by the matchmaker
        /// </summary>
        private int roomCount;

        /// <summary>
        /// List of rooms with their unique IDs
        /// </summary>
        private Dictionary<int, Process> rooms;

        private RoomHandler()
        {
        }

        public RoomHandler(string programName)
        {
            // Singleton
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Console.WriteLine($"RoomHandler already exist");
                return;
            }

            // Assignment
            roomName = programName;

            // Reset room dictionary
            ResetRoomDict();
        }

        /// <summary>
        /// Opens a room and assign its port
        /// </summary>
        public void OpenRoom()
        {
            ProcessStartInfo p_info = new()
            {
                UseShellExecute = true,
                FileName = roomName
            };
            for (int i = 0; i < roomCount; i++)
            {
                if (!rooms.ContainsKey(i))
                {
                    p_info.Arguments = $"-port {roomStartingPort + i}";
                    rooms.Add(i, Process.Start(p_info));
                }
            }
        }

        /// <summary>
        /// Opens <c>_roomCount</c> rooms and assign their ports
        /// </summary>
        /// <param name="_roomCount">Count of rooms to open</param>
        public void OpenRoom(int _roomCount)
        {
            ProcessStartInfo p_info = new()
            {
                UseShellExecute = true,
                FileName = roomName
            };
            for (int id = 0; id < _roomCount; id++)
            {
                p_info.Arguments = $"-port {roomStartingPort + id}";
                rooms.Add(id, Process.Start(p_info));
            }
        }

        /// <summary>
        /// Kills all room program and resets the dictionary
        /// </summary>
        private void ResetRoomDict()
        {
            if (rooms.Count > 0)
            {
                foreach (var room in rooms)
                {
                    room.Value.Kill(true);
                }
            }
            rooms.Clear();
        }

        private int FindMissingPort(int[] arr, int length)
        {
            int a = 0, b = length - 1;
            int mid = 0;
            while ((b - a) > 1)
            {
                mid = (a + b) / 2;
                if ((arr[a] - a) != (arr[mid] - mid))
                {
                    b = mid;
                }
                else if ((arr[b] - b) != (arr[mid] - mid))
                {
                    a = mid;
                }
            }
            return (arr[a] + 1);
        }
    }
}