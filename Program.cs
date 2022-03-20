using System;

namespace iSpyMatchmaker
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "iSpy Matchmaker";
            var server = new Server();
            server.Start(50, 8888);

            Console.ReadKey();
        }
    }
}