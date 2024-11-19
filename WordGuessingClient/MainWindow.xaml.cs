using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WordGuessingClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string uniqueID = string.Empty;
        private Client client = new Client();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void submitBtn_Click(object sender, RoutedEventArgs e)
        {
            uniqueID = Guid.NewGuid().ToString();

            string[] gameData = client.RunGameClient(serverAddress.Text, serverPort.Text, playerName.Text, uniqueID);

            wordBank.Content = "Word Bank: " + gameData[0];
            numOfWords.Content = "Words Remaining: " + gameData[1];
        }

        private void submitGuessBtn_Click(object sender, RoutedEventArgs e)
        {
            string gameData = client.RunGameClient(serverAddress.Text, serverPort.Text, playerName.Text, uniqueID, userGuess.Text);

            numOfWords.Content = "Words Remaining: " + gameData;
        }
    }
}
