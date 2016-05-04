using System;
using System.Windows;
using System.Windows.Input;
using ShipsClient.Common;
using ShipsClient.Enums;
using ShipsClient.Game;
using ShipsClient.Network;

namespace ShipsClient.BattleWindow
{
    /// <summary>
    /// Логика взаимодействия для PreBattleWindow.xaml
    /// </summary>
    public partial class PreBattleWindow : Window
    {
        private Board Board { get; set; }
        private int BattleId { get; set; }

        public static PreBattleWindow Form;

        public PreBattleWindow(int battleId = 0)
        {
            InitializeComponent();
            BattleId = battleId;
            Form = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Board = new Board(mainGrid);
            Board.AddRandomShips();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        private void _btMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void _btClose_Click(object sender, RoutedEventArgs e)
        {
            var window = new NotificationWindow.DialogWindow("Создание игры", "Вы уверены что хотите прервать создание игры?");
            if (window.ShowDialog() == false)
                return;

            Owner.Visibility = Visibility.Visible;
            Close();
        }

        private void _btBattle_Click(object sender, RoutedEventArgs e)
        {
            Opcodes opcode = Opcodes.CMSG_BATTLE_INITIALIZATION;
            if (BattleId != 0)
                opcode = Opcodes.CMSG_BATTLE_JOIN;

            var packet = new Packet((int)opcode);
            if (packet.Opcode == (int)Opcodes.CMSG_BATTLE_JOIN)
                packet.WriteInt32(BattleId);

            Board.WritePacket(packet);

            TCPSocket.Instance.SendPacket(packet);

            this.Visibility = Visibility.Hidden;
        }

        private void _btRandom_Click(object sender, RoutedEventArgs e)
        {
            Board.AddRandomShips();
        }

        public void InitialBattleWindow(int battleId, BattleResponse response, string myBoard = "", string oponentBoard = "")
        {
            var window = new ShipsClient.BattleWindow.BattleWindow() { Owner = this.Owner, MyBoard = this.Board, BattleId = battleId, MyTextBlock = myBoard, OponentTextBlock = oponentBoard };
            window.Show();

            if (response == BattleResponse.BATTLE_RESPONSE_JOIN_SUCCESS)
                window.StartBattle();

            Close();
        }
    }
}
