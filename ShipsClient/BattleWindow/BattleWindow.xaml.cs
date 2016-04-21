using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ShipsClient.Common;
using ShipsClient.Enums;
using ShipsClient.Game;
using ShipsClient.Network;
using Brush = System.Drawing.Brush;

namespace ShipsClient.BattleWindow
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class BattleWindow : Window
    {
        private bool _canShot;

        public Board MyBoard { get; set; }
        public Board OponentBoard { get; set; }
        public int BattleId { get; set; }
        public static BattleWindow Form;

        public bool CanShot
        {
            set
            {
                _canShot = value;
                OnCanShotChanged();
            }
            get
            {
                return _canShot;
            }
        }

        public string OponentTextBlock
        {
            set { _tbOponentTextBlock.Text = value; }
            get { return _tbOponentTextBlock.Text; }
        }

        public string MyTextBlock
        {
            set { _tbMyTextBlock.Text = value; }
            get { return _tbOponentTextBlock.Text; }
        }

        public BattleWindow()
        {
            InitializeComponent();
            Form = this;
            CanShot = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Owner.Visibility = Visibility.Visible;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _canShotCanvas.Visibility = Visibility.Hidden;

            // Мой поле
            MyBoard.SetGrid(_myGridChild);
            MyBoard.Status = BoardStatus.BOARD_STATUS_WAIT_OPONENT;
            MyBoard.Refresh();

            // Противник
            OponentBoard = new Board(_oponentGridChild, false);
            OponentBoard.Status = BoardStatus.BOARD_STATUS_WAIT_OPONENT;
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
            var window = new NotificationWindow.DialogWindow("Выйти из игры", "Вы действительно хотите завершить игру?");
            if (window.ShowDialog() == false)
                return;

            var packet = new Packet((int)Opcodes.CMSG_BATTLE_LEAVE);
            packet.WriteInt32(BattleId);
            ClientSocket.Instance.SendPacket(packet);

            Close();
        }

        public void StartBattle()
        {
            MyBoard.Status = BoardStatus.BOARD_STATUS_BATTLE;

            OponentBoard.Status = BoardStatus.BOARD_STATUS_BATTLE;
            OponentBoard.OnClick += OnBoardClick;

            _canShotCanvas.Visibility = Visibility.Visible;
        }

        public void EndBattle(bool leave = false)
        {
            string text = "";
            if (leave) // Противник вышел из игры, ответ сервера
                text = "Ваш противник покинул игру. Вы получаете очко победы!";

            var window = new NotificationWindow.NotificationWindow("Завершение игры", text);
            window.ShowDialog();
            Close();
        }

        private void OnBoardClick(object sender, BoardCellClickEventErgs e)
        {
            if (!CanShot)
                return;

            var packet = new Packet((int)Opcodes.CMSG_BATTLE_SHOT);
            packet.WriteInt32(BattleId);
            packet.WriteInt16((short)e.X);
            packet.WriteInt16((short)e.Y);
            ClientSocket.Instance.SendPacket(packet);
        }

        public void ShotResult(int x, int y, ShotResult result)
        {
            if (result == Enums.ShotResult.SHOT_RESULT_MISSED)
                CanShot = false;

            OponentBoard.UpdateCellState(x, y, result);
        }

        private void OnCanShotChanged()
        {
            var status = "";
            if (_canShot)
                status = "arrow_green.png";
            else
                status = "arrow_red.png";

            _canShotCanvas.Background = new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/Resources/Battle/{status}")));
        }
    }
}
