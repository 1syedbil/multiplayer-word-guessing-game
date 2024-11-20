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
            uniqueID = Guid.NewGuid().ToString();

            string[] gameData = client.RunGameClient(serverAddress.Text, serverPort.Text, playerName.Text, uniqueID, timeLimitValue.Text);

            wordBank.Text = gameData[0];
            numOfWords.Text = gameData[1];

            StartTimer();
            submitBtn.Visibility = Visibility.Collapsed;
        }

        private void submitGuessBtn_Click(object sender, RoutedEventArgs e)
        {
            string gameData = client.RunGameClient(serverAddress.Text, serverPort.Text, playerName.Text, uniqueID, userGuess.Text, timeLimitValue.Text);

            if (gameData == "Game Finished")
            {
                MessageBox.Show("Game Finished. You won!");
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
                         if (client.RequestTimerStatus(serverAddress.Text, serverPort.Text, uniqueID) == "Game Finished")
                         {
                             MessageBox.Show("Game Finished. Time's up!");
                         }
                     }
                    time = time.Add(TimeSpan.FromSeconds(-1));
                 }, Application.Current.Dispatcher);

            timer.Start();
        }
    }
}
