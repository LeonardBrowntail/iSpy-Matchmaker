using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace iSpyMatchmaker
{
    internal class Program
    {
        //Matchmaker server
        private static readonly ushort matchmakerPort = 7777;

        //Room manager
        private static bool isRunning = false;

        private static void Main(string[] args)
        {
            Console.Title = "iSpy Matchmaker";
            Console.WriteLine($"Please insert the server build file name (include extension!): ");
            string programName = Console.ReadLine();
            Console.WriteLine($"How many rooms do you want to open?");
            string roomCount = Console.ReadLine();

            isRunning = true;
            Thread mainThread = new(new ThreadStart(MainThread));
            mainThread.Start();

            Matchmaker.Singleton.Initialize(int.Parse(roomCount), matchmakerPort);
            Matchmaker.Singleton.Start();
            RoomHandler.Singleton.Initialize(programName);
            RoomHandler.Singleton.OpenRooms(int.Parse(roomCount));

            string input;
            do
            {
                input = Console.ReadLine();
            } while (input != "quit");

            Matchmaker.Singleton.Stop();
            RoomHandler.Singleton.Close();

            Environment.Exit(-1);
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread has started, running at {Consts.TICKS_PER_SEC} ticks per second");
            DateTime nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (nextLoop < DateTime.Now)
                {
                    Update();

                    nextLoop = nextLoop.AddMilliseconds(Consts.MS_PER_TICK);

                    if (nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }

        private static void Update()
        {
            ThreadManager.UpdateMain();
        }
    }
}