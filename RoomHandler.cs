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
        private static RoomHandler singleton = null;

        private bool initialized = false;

        public static RoomHandler Singleton
        { get { if (singleton == null) singleton = new(); return singleton; } }

        /// <summary>
        /// Matchmaker current directory
        /// </summary>
        private readonly string filePath = Directory.GetCurrentDirectory();

        /// <summary>
        /// 1st room port number is set to 1 + matchmaker's port
        /// </summary>
        private readonly ushort roomStartingPort = (ushort)(Matchmaker.Singleton.Port + 1);

        /// <summary>
        /// Room program name, a.k.a the Unity server program
        /// </summary>
        private string roomName;

        /// <summary>
        /// The number of rooms that are started by the matchmaker
        /// </summary>
        private int roomCount = 0;

        /// <summary>
        /// List of rooms with their unique IDs
        /// </summary>
        private Dictionary<int, Process> rooms = null;

        private Dictionary<int, ServerDataEntry> rooms_entry = null;

        /// <summary>
        /// Main database, contains the latest updated state for every connected servers
        /// </summary>
        public static Dictionary<int, ServerDataEntry> Entries => singleton.rooms_entry;

        private RoomHandler()
        {
        }

        public void Initialize(string programName)
        {
            roomName = programName;
            ResetRoomDict();
            ResetEntryDatabase();

            initialized = true;
        }

        /// <summary>
        /// Opens <c>_roomCount</c> of rooms
        /// </summary>
        /// <param name="_roomCount">The number of rooms to be opened</param>
        public void OpenRooms(int _roomCount)
        {
            if (!initialized)
            {
                Console.WriteLine($"Roomhandler is not yet initialized!");
                return;
            }
            ProcessStartInfo p_info = new()
            {
                UseShellExecute = true,
                FileName = roomName
            };
            Console.WriteLine($"Creating rooms...");
            for (int id = 0; id < _roomCount; id++)
            {
                p_info.Arguments = $"-m_port {Matchmaker.Singleton.Port} -port {roomStartingPort + id}";
                rooms.Add(roomStartingPort + id, Process.Start(p_info));
                Console.WriteLine($"Creating room(id={id}) with assigned port {roomStartingPort + id}");
            }
        }

        /// <summary>
        /// Kills all room program and resets the dictionary
        /// </summary>
        private void ResetRoomDict()
        {
            if (rooms == null) rooms = new Dictionary<int, Process>();
            if (rooms.Count > 0)
            {
                foreach (var room in rooms)
                {
                    room.Value.Kill(true);
                }
                rooms.Clear();
            }
            Console.WriteLine($"Room dictionary reset!");
        }

        private void ResetEntryDatabase()
        {
            if (rooms_entry == null) rooms_entry = new();
            if (rooms_entry.Count > 0)
            {
                rooms_entry.Clear();
            }
            Console.WriteLine($"Database dictionary reset!");
        }

        public void TerminateRoom(int id)
        {
            Matchmaker.Servers[id].Disconnect();
            singleton.rooms[id].Kill();
            singleton.rooms.Remove(id);
        }

        public void Close()
        {
            ResetRoomDict();
            rooms = null;
            rooms_entry = null;
            Console.WriteLine($"Closing all rooms");
        }

        //Deprecated
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

        /// <summary>
        /// Opens a room and assign its port
        /// </summary>
        public void OpenRoom()
        {
            if (!initialized)
            {
                Console.WriteLine($"Roomhandler is not yet initialized!");
                return;
            }
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
                    rooms.Add(roomStartingPort + i, Process.Start(p_info));
                }
                else
                {
                    Console.WriteLine($"port {roomStartingPort + i} is being used");
                }
            }
        }
    }
}