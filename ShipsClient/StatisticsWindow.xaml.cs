using System;
using System.Windows;
using ShipsClient.Common;
using ShipsClient.Network;

namespace ShipsClient
{
    /// <summary>
    /// Логика взаимодействия для StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        public static StatisticsWindow Form;
        public StatisticsWindow()
        {
            InitializeComponent();
            Form = this;

            TCPSocket.Instance.SendPacket(new Packet((int)Opcodes.CMSG_GET_STATISTICS));
        }

        public void RecivieStatistics(uint lastGame, uint wins, uint loose)
        {
            _tbLastGameValue.Text = lastGame > 0 ? string.Format("{0:dd-MM-yyyy}", Time.UnixTimeStampToDateTime(lastGame)) : "00-00-0000";
            _tbWinValue.Text = wins.ToString();
            _tbLooseValue.Text = loose.ToString();
            _tbTotalGamesValue.Text = (wins + loose).ToString();
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
    }
}
