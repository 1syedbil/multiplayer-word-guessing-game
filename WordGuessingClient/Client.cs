using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WordGuessingClient
{
    internal class Client
    {

        public string RunGameClient(string ip, string port, string name)
        {
            Int32.TryParse(port, out int serverPort);

            return ConnectToGameServer(ip, serverPort, name);
        }

        private string ConnectToGameServer(string ip, int port, string name)
        {
            try
            {
                TcpClient player = new TcpClient(ip, port);

                Byte[] userName = Encoding.ASCII.GetBytes(name);

                NetworkStream stream = player.GetStream();

                stream.Write(userName, 0, userName.Length);

                Byte[] serverResponse = new Byte[100];

                stream.Read(serverResponse, 0, serverResponse.Length);

                string serverMessage = Encoding.ASCII.GetString(serverResponse);

                stream.Close();
                player.Close();

                return serverMessage;
            }
            catch (ArgumentNullException e)
            {
                return e.ToString();
            }
            catch (SocketException e)
            {
                return e.ToString();
            }
        }

    }
}
