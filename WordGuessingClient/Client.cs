using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WordGuessingClient
{
    internal class Client
    {

        public string[] RunGameClient(string ip, string port, string name, string uniqueID, string timeLimit)
        {
            Int32.TryParse(port, out int serverPort);

            return ConnectToGameServer(ip, serverPort, name, uniqueID, timeLimit);
        }

        public string RunGameClient(string ip, string port, string name, string uniqueID, string guess, string timeLimit)
        {
            Int32.TryParse(port, out int serverPort);

            return ConnectToGameServer(ip, serverPort, name, uniqueID, guess, timeLimit);
        }

        public string RequestTimerStatus(string ip, string port, string uniqueID)
        {
            Int32.TryParse(port, out int serverPort);

            return ConnectToGameServer(ip, serverPort, uniqueID);
        }

        public string RequestRestart(string ip, string port, string uniqueID)
        {
            Int32.TryParse(port, out int serverPort);

            return ConnectToGameServer(ip, serverPort, uniqueID, "Restart");
        }

        private string ConnectToGameServer(string ip, int port, string uniqueID, string request)
        {
            byte[] serverResponse = new byte[100];
            byte[] userInfo = Encoding.ASCII.GetBytes(uniqueID + "," + request);

            TcpClient player = new TcpClient(ip, port);

            NetworkStream stream = player.GetStream();

            stream.Write(userInfo, 0, userInfo.Length);

            stream.Read(serverResponse, 0, serverResponse.Length);

            string serverMessage = Encoding.ASCII.GetString(serverResponse).Trim('\0');

            stream.Close();
            player.Close();

            return serverMessage;
        }

        private string ConnectToGameServer(string ip, int port, string uniqueID)
        {
            try
            {
                byte[] serverResponse = new byte[100];
                byte[] userInfo = Encoding.ASCII.GetBytes(uniqueID);

                TcpClient player = new TcpClient(ip, port);

                NetworkStream stream = player.GetStream();

                stream.Write(userInfo, 0, userInfo.Length);

                stream.Read(serverResponse, 0, serverResponse.Length);

                string serverMessage = Encoding.ASCII.GetString(serverResponse).Trim('\0');

                stream.Close();
                player.Close();

                return serverMessage;
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private string ConnectToGameServer(string ip, int port, string name, string uniqueID, string guess, string timeLimit)
        {
            try
            {
                TcpClient player = new TcpClient(ip, port);

                string info = uniqueID + "," + name + "," + timeLimit + "," + guess;

                byte[] userInfo = Encoding.ASCII.GetBytes(info);

                NetworkStream stream = player.GetStream();

                stream.Write(userInfo, 0, userInfo.Length);

                byte[] serverResponse = new byte[100];

                stream.Read(serverResponse, 0, serverResponse.Length);

                string serverMessage = Encoding.ASCII.GetString(serverResponse).Trim('\0');

                stream.Close();
                player.Close();

                return serverMessage;
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private string[] ConnectToGameServer(string ip, int port, string name, string uniqueID, string timeLimit)
        {
            try
            {
                TcpClient player = new TcpClient(ip, port);

                string info = uniqueID + "," + name + "," + timeLimit;

                byte[] userInfo = Encoding.ASCII.GetBytes(info); 

                NetworkStream stream = player.GetStream();

                stream.Write(userInfo, 0, userInfo.Length);

                byte[] serverResponse = new byte[100];

                stream.Read(serverResponse, 0, serverResponse.Length);

                string serverMessage = Encoding.ASCII.GetString(serverResponse).Trim('\0');

                string[] gameInfo = serverMessage.Split(',');

                stream.Close();
                player.Close();

                return gameInfo;
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

    }
}
