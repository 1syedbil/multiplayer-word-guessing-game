using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordGuessingServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server is running...");

            Server server = new Server();
            server.StartGameServer();
        }
    }
}
