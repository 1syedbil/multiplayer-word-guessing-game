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

        public string[] RunGameClient(string ip, string port, string name, string uniqueID)
        {
            Int32.TryParse(port, out int serverPort);

            return ConnectToGameServer(ip, serverPort, name, uniqueID);
        }

        private string[] ConnectToGameServer(string ip, int port, string name, string uniqueID)
        {
            try
            {
                TcpClient player = new TcpClient(ip, port);

                string info = uniqueID + "," + name;

                byte[] userInfo = Encoding.ASCII.GetBytes(info); 

                NetworkStream stream = player.GetStream();

                stream.Write(userInfo, 0, userInfo.Length);

                byte[] serverResponse = new byte[100];

                stream.Read(serverResponse, 0, serverResponse.Length);

                string serverMessage = Encoding.ASCII.GetString(serverResponse);

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
