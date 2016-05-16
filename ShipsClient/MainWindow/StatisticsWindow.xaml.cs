using System;
using System.Windows;
using ShipsClient.Common;
using ShipsClient.Network;

namespace ShipsClient.MainWindow
{
    /// <summary>
    /// Логика взаимодействия для StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        private uint LastGame { get; set; }
        private uint Wins { get; set; }
        private uint Loose { get; set; }

        public static StatisticsWindow Form;
        public StatisticsWindow(uint lastGame, uint wins, uint loose)
        {
            InitializeComponent();
            Form = this;

            LastGame = lastGame;
            Wins = wins;
            Loose = loose;
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void _btClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void _btMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _tbLastGameValue.Text = LastGame > 0 ? $"{Time.UnixTimeStampToDateTime(LastGame):dd-MM-yyyy}" : "00-00-0000";
            _tbWinValue.Text = Wins.ToString();
            _tbLooseValue.Text = Loose.ToString();
            _tbTotalGamesValue.Text = (Wins + Loose).ToString();
        }
    }
}
