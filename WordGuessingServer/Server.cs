using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace WordGuessingServer
{
    internal class Server
    {
        private Int32 port = 20000;

        public void StartGameServer()
        {
            TcpListener gameServer = null;
            string address = RetrieveAddress();
            IPAddress ip = IPAddress.Parse(address);

            try
            {
                gameServer = new TcpListener(ip, port);

                gameServer.Start();

                while (true)
                {
                    TcpClient gameClient = gameServer.AcceptTcpClient();

                    Task.Run(() => RunGameLogic(gameClient));
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Exception thrown: {0}", e);
            }
            finally
            {
                gameServer.Stop();
            }
        }

        private void RunGameLogic(TcpClient client)
        {
            Byte[] dataBuffer = new Byte[100];
            string playerName = null;
            string message = null;
            Byte[] serverMessage = new Byte[100];

            NetworkStream stream = client.GetStream();

            stream.Read(dataBuffer, 0, dataBuffer.Length);

            playerName = Encoding.ASCII.GetString(dataBuffer);

            message = "Hello " + playerName;

            serverMessage = Encoding.ASCII.GetBytes(message);

            stream.Write(serverMessage, 0, serverMessage.Length);

            client.Close();
        }
        
        private string RetrieveAddress()
        {
            string address = null;
            string host = Dns.GetHostName();
            IPHostEntry entry = Dns.GetHostEntry(host);

            address = entry.AddressList[3].ToString();

            return address;
        }

    }
}
