/*
* FILE          : Program.cs
* PROJECT       : WindowsProg_A5
* PROGRAMMER    : Bilal Syed
* FIRST VERSION : 2024-11-14
* DESCRIPTION   : This is the file containing the code for Main which is what actually 
*                 initializes the Server object so that all the game logic can be performed.
*                
*/

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
