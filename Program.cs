using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace iSpyMatchmaker
{
    internal class Program
    {
        private static string filePath = Directory.GetCurrentDirectory();
        private static List<Process> processes = new();
        private static ushort startingPort = 27016;

        private static void CreateInstances()
        {
            Console.WriteLine("Open how many servers? ");
            var num = Console.ReadLine();
            if (num.Length < 1)
            {
                return;
            }
            int programNum;
            if (int.TryParse(num, out programNum))
            {
                ProcessStartInfo p_info = new();
                p_info.UseShellExecute = true;
                p_info.CreateNoWindow = false;
                p_info.WindowStyle = ProcessWindowStyle.Normal;
                p_info.FileName = filePath + "\\TestProgram.exe";
                for (int i = 0; i < programNum; i++)
                {
                    p_info.Arguments = $"-port {startingPort + i}";
                    processes.Add(Process.Start(p_info));
                }
            }
        }

        private static void Main(string[] args)
        {
            Console.Title = "iSpy Matchmaker";
            CreateInstances();
            //var server = new Server();
            //server.Start(50, 27015);

            Console.ReadKey();
        }
    }
}