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
using System.Runtime.Remoting;
using System.Diagnostics;

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
        private Dictionary<string, bool> statusOfGames = new Dictionary<string, bool>();
        private Dictionary<string, bool> gameRestarts = new Dictionary<string, bool>();
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

        private async void RunGameLogic(TcpClient client)
        {
            byte[] clientData = new byte[100];
            string playerMessage = null;
            string[] playerInfo = null;
            byte[] serverMessage = new byte[100];

            NetworkStream stream = client.GetStream();

            stream.Read(clientData, 0, clientData.Length);

            playerMessage = Encoding.ASCII.GetString(clientData).Trim('\0'); ;
            Console.WriteLine(playerMessage);

            playerInfo = playerMessage.Split(',');

            int playerExists = CheckPlayerInfo(playerInfo);

            if (playerExists == 0 || gameRestarts[playerInfo[0]])
            {
                if (playerExists == 0)
                {
                    players.Add(playerInfo[0], playerInfo[1]);
                }

                string gameData = string.Empty;

                gameData = InitializeGame(gameData, playerInfo, playerExists);

                Task countdown = Countdown(timeLimits[playerInfo[0]], playerInfo);

                serverMessage = Encoding.ASCII.GetBytes(gameData);

                stream.Write(serverMessage, 0, serverMessage.Length);

                await countdown;
            }
            else if (playerExists == 1)
            {
                string newGameData = string.Empty;

                if (playerInfo.Length > 1 && playerInfo[1] == "Restart")
                {
                    ResetPlayer(playerInfo);

                    newGameData = "Game Restarted";
                    Console.WriteLine(newGameData);
                }
                else if (statusOfGames[playerInfo[0]] == false)
                {
                    newGameData = "Game Finished";
                    Console.WriteLine(newGameData);
                }
                else
                {
                    newGameData = CheckGuess(playerInfo);
                }

                serverMessage = Encoding.ASCII.GetBytes(newGameData); 

                stream.Write(serverMessage, 0, serverMessage.Length);
            }

            client.Close();
        }

        private void ResetPlayer(string[] playerInfo)
        {
            gameData = new string[100];
            correctWords[playerInfo[0]] = new string[100];
            statusOfGames[playerInfo[0]] = true;
            timeLimits[playerInfo[0]] = 0;
            remainingWords[playerInfo[0]] = string.Empty;
            gameRestarts[playerInfo[0]] = true;
        }

        private async Task Countdown(double timeMinutes, string[] playerInfo)
        {
            int timeMilliseconds = (int)(timeMinutes * 60000);

            await Task.Delay(timeMilliseconds);

            Console.WriteLine("Timer finished.");

            statusOfGames[playerInfo[0]] = false;
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

                if (j == 0)
                {
                    return "Game Finished";
                }

                remainingWords[playerInfo[0]] = j.ToString();

                return remainingWords[playerInfo[0]];
            }
            else
            {
                return remainingWords[playerInfo[0]];
            }
        }

        private string InitializeGame(string data, string[] playerInfo, int playerExists)
        {
            string[] words = new string[100];

            data = InitializeGameData(data);
            gameData = data.Split(',');

            if (playerExists == 0)
            {
                remainingWords.Add(playerInfo[0], gameData[1]);

                Double.TryParse(playerInfo[2], out double time);
                timeLimits.Add(playerInfo[0], time);

                statusOfGames.Add(playerInfo[0], true);
                gameRestarts.Add(playerInfo[0], false);
            }
            else if (playerExists == 1)
            {
                remainingWords[playerInfo[0]] = gameData[1];

                Double.TryParse(playerInfo[2], out double time);
                timeLimits[playerInfo[0]] = time;

                statusOfGames[playerInfo[0]] = true;
                gameRestarts[playerInfo[0]] = false;
            }

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

            if (playerExists == 0)
            {
                correctWords.Add(playerInfo[0], words);
            }
            else if (playerExists == 1)
            {
                correctWords[playerInfo[0]] = words;
            }

            return data;
        }

        private string InitializeGameData(string gameData)
        {
            Random rand = new Random(); 

            string[] files = Directory.GetFiles("WordBanks");

            string file = files[rand.Next(files.Length)];

            string[] a = File.ReadAllLines(file);

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
