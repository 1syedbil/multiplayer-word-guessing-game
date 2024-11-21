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
using System.Windows.Threading;

namespace WordGuessingClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string uniqueID = string.Empty;
        private Client client = new Client();
        DispatcherTimer timer;
        TimeSpan time;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void submitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (uniqueID == string.Empty)
            {
                uniqueID = Guid.NewGuid().ToString();
            }

            string[] gameData = client.RunGameClient(serverAddress.Text, serverPort.Text, playerName.Text, uniqueID, timeLimitValue.Text);

            wordBank.Text = gameData[0];
            numOfWords.Text = gameData[1];

            StartTimer();
            submitBtn.Visibility = Visibility.Hidden;
            rtLabel.Visibility = Visibility.Visible;
            userGuess.Visibility = Visibility.Visible;
            wrLabel.Visibility = Visibility.Visible;
            wbLabel.Visibility = Visibility.Visible;
            submitGuessBtn.Visibility = Visibility.Visible;
        }

        private void submitGuessBtn_Click(object sender, RoutedEventArgs e)
        {
            string gameData = client.RunGameClient(serverAddress.Text, serverPort.Text, playerName.Text, uniqueID, userGuess.Text, timeLimitValue.Text);

            if (gameData == "Game Finished")
            {
                timer.Stop();
                MessageBoxResult userChoice = MessageBox.Show("You won! Would you like to play again?", "Game Finished", MessageBoxButton.YesNo);
                GameRestartChoice(userChoice);
                return;
            }
            
            numOfWords.Text = gameData;
        }

        private void timeLimit_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timeLimitValue.Text = Math.Round(timeLimit.Value, 1).ToString();
        }

        private void StartTimer()
        {
            //the code for this method related to the timer is from https://stackoverflow.com/questions/16748371/how-to-make-a-wpf-countdown-timer 
            InitializeComponent();

            time = TimeSpan.FromMinutes(Math.Round(timeLimit.Value, 1));

            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
                 {
                    tbTime.Text = time.ToString("c");
                     if (time == TimeSpan.Zero) 
                     { 
                         timer.Stop();
                         //this if-statement is my own code
                         if (client.RequestTimerStatus(serverAddress.Text, serverPort.Text, uniqueID) == "Game Finished")
                         {
                             MessageBoxResult userChoice = MessageBox.Show("Time's up! Would you like to play again?", "Game Finished", MessageBoxButton.YesNo);
                             GameRestartChoice(userChoice);
                         }
                     }
                    time = time.Add(TimeSpan.FromSeconds(-1));
                 }, Application.Current.Dispatcher);

            timer.Start();
        }

        private void GameRestartChoice(MessageBoxResult userChoice)
        {
            if (userChoice == MessageBoxResult.Yes)
            {
                string message = client.RequestRestart(serverAddress.Text, serverPort.Text, uniqueID);

                if (message == "Game Restarted")
                {
                    timer.Stop();

                    wordBank.Text = string.Empty;
                    numOfWords.Text = string.Empty;
                    tbTime.Text = string.Empty;
                    userGuess.Text = string.Empty;
                    timeLimit.Value = 0;

                    submitBtn.Visibility = Visibility.Visible;
                    rtLabel.Visibility = Visibility.Hidden;
                    userGuess.Visibility = Visibility.Hidden;
                    wrLabel.Visibility = Visibility.Hidden;
                    wbLabel.Visibility = Visibility.Hidden;
                    submitGuessBtn.Visibility = Visibility.Hidden;
                }
            }
            else if (userChoice == MessageBoxResult.No)
            {
                Close();
            }
        }
    }
}
