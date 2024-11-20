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
        private TcpListener gameServer = null;
        private Dictionary<string, string> players = new Dictionary<string, string>();
        private Dictionary<string, string> remainingWords = new Dictionary<string, string>();
        private Dictionary<string, string[]> correctWords = new Dictionary<string, string[]>();
        private Dictionary<string, double> timeLimits = new Dictionary<string, double>();
        private string[] gameData = new string[100];

        public void StartGameServer()
        {
            gameServer = null;
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

                Countdown(timeLimits[playerInfo[0]], stream);
            }
            else if (CheckPlayerInfo(playerInfo) == 1)
            {
                string newGameData = CheckGuess(playerInfo);

                serverMessage = Encoding.ASCII.GetBytes(newGameData); 

                stream.Write(serverMessage, 0, serverMessage.Length);
            }

            client.Close();
        }

        private async Task Countdown(double timeMinutes, NetworkStream stream)
        {
            int timeMilliseconds = (int)(timeMinutes * 60000) + 1000;
            await Task.Delay(timeMilliseconds);

            Console.WriteLine("Timer finished.");
        }

        private string CheckGuess(string[] playerInfo)
        {
            if (correctWords[playerInfo[0]].AsQueryable().Contains(playerInfo[3].Trim('\0')))
            {
                for (int i = 0; i < correctWords[playerInfo[0]].Length; i++)
                {
                    if (correctWords[playerInfo[0]][i] == playerInfo[3].Trim('\0'))
                    {
                        correctWords[playerInfo[0]][i] = string.Empty;
                        break;
                    }
                }

                Int32.TryParse(remainingWords[playerInfo[0]], out int j);
                j--;

                remainingWords[playerInfo[0]] = j.ToString();

                return remainingWords[playerInfo[0]];
            }
            else
            {
                return remainingWords[playerInfo[0]];
            }
        }

        private string InitializeGame(string data, string[] playerInfo)
        {
            string[] words = new string[100];
            data = InitializeGameData(data);
            gameData = data.Split(',');
            remainingWords.Add(playerInfo[0], gameData[1]);
            Double.TryParse(playerInfo[2], out double time);
            timeLimits.Add(playerInfo[0], time);

            int count = 0;

            for (int i = 0; i < gameData.Length; i++)
            {
                if (gameData[i].Length >= 80 || Int32.TryParse(gameData[i], out int j) || gameData[i] == string.Empty)
                {
                    continue;
                }

                words[count] = gameData[i];
                count++;
            }

            correctWords.Add(playerInfo[0], words);

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
