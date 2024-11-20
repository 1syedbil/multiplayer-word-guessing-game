using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;

namespace WordGuessingServer
{
    internal class Server
    {
        private Int32 port = 60000;
        private Dictionary<string, string> players = new Dictionary<string, string>();
        private Dictionary<string, string> a = new Dictionary<string, string>();
        private Dictionary<string, string[]> b = new Dictionary<string, string[]>();
        private string[] gameData = new string[100];
        private string[] correctWords = new string[100];

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
            byte[] clientData = new byte[100];
            string playerMessage = null;
            string[] playerInfo = null;
            byte[] serverMessage = new byte[100];

            NetworkStream stream = client.GetStream();

            stream.Read(clientData, 0, clientData.Length);

            playerMessage = Encoding.ASCII.GetString(clientData);
            Console.WriteLine(playerMessage);

            playerInfo = playerMessage.Split(',');

            if (CheckPlayerInfo(playerInfo) == 0)
            {
                players.Add(playerInfo[0], playerInfo[1]);

                string gameData = string.Empty;

                gameData = InitializeGame(gameData, playerInfo);

                serverMessage = Encoding.ASCII.GetBytes(gameData);

                stream.Write(serverMessage, 0, serverMessage.Length);
            }
            else if (CheckPlayerInfo(playerInfo) == 1)
            {
                string newGameData = CheckGuess(playerInfo);

                serverMessage = Encoding.ASCII.GetBytes(newGameData); 

                stream.Write(serverMessage, 0, serverMessage.Length);
            }

            client.Close();
        }

        private string CheckGuess(string[] playerInfo)
        {
            if (b[playerInfo[0]].AsQueryable().Contains(playerInfo[3].Trim('\0')))
            {
                for (int i = 0; i < correctWords.Length; i++)
                {
                    if (b[playerInfo[0]][i] == playerInfo[3].Trim('\0'))
                    {
                        b[playerInfo[0]][i] = string.Empty;
                        break;
                    }
                }

                Int32.TryParse(a[playerInfo[0]], out int j);
                j--;

                a[playerInfo[0]] = j.ToString();

                return a[playerInfo[0]];
            }
            else
            {
                return a[playerInfo[0]];
            }
        }

        private string InitializeGame(string data, string[] playerInfo)
        {
            data = InitializeGameData(data);

            gameData = data.Split(',');
            a.Add(playerInfo[0], gameData[1]);

            int count = 0;

            for (int i = 0; i < gameData.Length; i++)
            {
                if (gameData[i].Length >= 80 || Int32.TryParse(gameData[i], out int j) || gameData[i] == string.Empty)
                {
                    continue;
                }

                correctWords[count] = gameData[i];
                count++;
            }

            b.Add(playerInfo[0], correctWords);

            return data;
        }

        private string InitializeGameData(string gameData)
        {
            string[] a = File.ReadAllLines("WordBanks/wordBank1.txt");

            foreach (string data in a)
            {
                gameData += data;
                gameData += ',';
            }

            return gameData;
        }

        private int CheckPlayerInfo(string[] playerInfo)
        {
            int playerExists = 0;
            
            if (players == null)
            {
                playerExists = 0;
            }
            else if (!players.ContainsKey(playerInfo[0]))
            {
                playerExists = 0;
            }
            else if (players.ContainsKey(playerInfo[0]))
            {
                playerExists = 1;
            }

            return playerExists;
        }
        
        private string RetrieveAddress()
        {
            string ipAddress = null;

            NetworkInterface[] addresses = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface address in addresses)
            {
                if (address.Name == "Wi-Fi" || address.Name == "Ethernet")
                {
                    foreach (UnicastIPAddressInformation ipInfo in address.GetIPProperties().UnicastAddresses)
                    {
                        if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddress = ipInfo.Address.ToString();
                            return ipAddress;
                        }
                    }
                }
            }

            return ipAddress;
        }

    }
}
