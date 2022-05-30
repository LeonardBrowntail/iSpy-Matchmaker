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

        public static RoomHandler Singleton
        { get { if (singleton == null) singleton = new(); return singleton; } }

        private bool initialized = false;

        /// <summary>
        /// Matchmaker current directory
        /// </summary>
        private readonly string filePath = $"{Directory.GetCurrentDirectory()}\\server";

        /// <summary>
        /// 1st room port number is set to 1 + matchmaker's port
        /// </summary>
        private readonly ushort roomStartingPort = (ushort)(Matchmaker.Singleton.Port + 1);

        /// <summary>
        /// Room program name, a.k.a the Unity server program
        /// </summary>
        private string roomName;

        /// <summary>
        /// List of rooms with their unique IDs
        /// </summary>
        private Dictionary<int, Process> rooms = null;

        /// <summary>
        /// Main database, contains the latest updated state for every connected servers
        /// </summary>
        public Dictionary<int, ServerDataEntry> Entries { get; private set; }

        private RoomHandler()
        {
        }

        /// <summary>
        /// Initialize <c>RoomHandler</c> singleton
        /// </summary>
        /// <param name="programName">Unity server build name</param>
        public void Initialize(string programName)
        {
            if (string.IsNullOrEmpty(programName)) return;
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
                Console.WriteLine($"=========================== Error! ===============================\n" +
                    $"Roomhandler is not yet initialized!");
                return;
            }
            ProcessStartInfo p_info = new()
            {
                UseShellExecute = true,

                FileName = $"{filePath}\\{roomName}"
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
                    try
                    {
                        room.Value.Kill(true);
                        room.Value.Dispose();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                rooms.Clear();
            }
            Console.WriteLine($"Room dictionary reset!");
        }

        /// <summary>
        /// Resets entry dictionary
        /// </summary>
        private void ResetEntryDatabase()
        {
            if (Entries == null) Entries = new();
            if (Entries.Count > 0)
            {
                Entries.Clear();
            }
            Console.WriteLine($"Database dictionary reset!");
        }

        /// <summary>
        /// Terminates a room with a certain ID
        /// </summary>
        /// <param name="id">room's id</param>
        public void TerminateRoom(int id)
        {
            Matchmaker.Servers[id].Disconnect();
            rooms[id].Kill();
            rooms.Remove(id);
        }

        /// <summary>
        /// Cleans all room for disposal
        /// </summary>
        public void Close()
        {
            ResetRoomDict();
            rooms = null;
            Entries = null;
            Console.WriteLine($"Closing all rooms");
        }

        /// <summary>
        /// Checks whether the directory and the server build exists
        /// </summary>
        /// <returns><see langword="true"/>, if the directory and server program exists</returns>
        public bool ProgramCheck(string _programName)
        {
            // checks if the directory exists
            if (!Directory.Exists(filePath))
            {
                Console.WriteLine($"=========================== Error! ===============================\n" +
                    $"Directory {filePath} doesn't exist, creating and exiting program...\n" +
                    $"Please place the server build inside the newly created \"server\" folder.");
                Directory.CreateDirectory(filePath);
                return false;
            }
            // checks if the program exists
            if (!File.Exists($"{filePath}\\{_programName}"))
            {
                Console.WriteLine($"=========================== Error! ===============================\n" +
                    $"Roomname is invalid or the program does not exist, exiting program\n" +
                    $"Makes sure you have input the correct name and has the server program in the server folder");
                return false;
            }
            return true;
        }
    }
}