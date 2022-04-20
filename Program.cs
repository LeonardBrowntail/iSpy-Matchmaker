using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace iSpyMatchmaker
{
    internal class Program
    {
        //Matchmaker server
        private static Matchmaker matchmaker;

        //Room manager

        private static Dictionary<int, Process> processes;

        private static ushort startingPort = 27016;
        private static string programName = "";

        private static void Main(string[] args)
        {
            Console.Title = "iSpy Matchmaker";
            Console.Write($"Please insert the server build file name (include extension!): ");
            programName = Console.ReadLine();

            RoomHandler.instance = new RoomHandler(programName);

            bool running = false;
            while (running)
            {
                var input = Console.ReadLine();
                if (input == "exit")
                {
                    running = false;
                }
            }
        }
    }
}