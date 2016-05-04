using System;
using System.CodeDom;
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

            if (MyBoard.Status == BoardStatus.BOARD_STATUS_BATTLE || MyBoard.Status == BoardStatus.BOARD_STATUS_WAIT_OPONENT)
            {
                var packet = new Packet((int)Opcodes.CMSG_BATTLE_LEAVE);
                packet.WriteInt32(BattleId);
                TCPSocket.Instance.SendPacket(packet);
            }

            Close();
        }

        public void StartBattle()
        {
            MyBoard.Status = BoardStatus.BOARD_STATUS_BATTLE;

            OponentBoard.Status = BoardStatus.BOARD_STATUS_BATTLE;
            OponentBoard.OnClick += OnBoardClick;

            _canShotCanvas.Visibility = Visibility.Visible;
        }

        public void EndBattle(bool leave = false, bool win = false)
        {
            string text = "";
            if (leave) // Противник вышел из игры, ответ сервера
                text = "Ваш противник покинул игру. Вы получаете очко победы!";
            else if (win)
                text = "Вы выйграли этот бой! Поздравляем!";
            else
                text = "Вы проиграли этот бой...";

            var window = new NotificationWindow.NotificationWindow("Завершение игры", text);
            window.ShowDialog();
            Close();
        }

        private void OnBoardClick(object sender, BoardCellClickEventErgs e)
        {
            if (!CanShot)
                return;

            _canShot = false;
            var packet = new Packet((int)Opcodes.CMSG_BATTLE_SHOT);
            packet.WriteInt32(BattleId);
            packet.WriteUInt8((byte)e.X);
            packet.WriteUInt8((byte)e.Y);
            TCPSocket.Instance.SendPacket(packet, true);
        }

        public void ShotResult(int x, int y, ShotResult result)
        {
            if (result == Enums.ShotResult.SHOT_RESULT_MISSED)
                CanShot = false;
            else
                _canShot = true;

            OponentBoard.UpdateCellState(x, y, result);
        }

        public void ShipDrowned(Ship ship)
        {
            if (ship == null)
                return;

            OponentBoard.AddShip(ship, ship.X, ship.Y, true);
            OponentBoard.UpdateCellState(ship.X, ship.Y, Enums.ShotResult.SHOT_RESULT_SHIP_DROWNED);
        }
        
        private void OnCanShotChanged()
        {
            var status = "";
            status = _canShot ? "arrow_green.png" : "arrow_red.png";
            _canShotCanvas.Background = new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/Resources/Battle/{status}")));
        }

        public void UpdateBoard(int x, int y, ShotResult result)
        {
            if (result == Enums.ShotResult.SHOT_RESULT_SHIP_DROWNED)
            {
                var ship = MyBoard.GetShipAt(x, y);
                if (ship != null)
                {
                    MyBoard.ResetShipRegion(x, y);
                    MyBoard.UpdateCellState(ship.X, ship.Y, Enums.ShotResult.SHOT_RESULT_SHIP_DROWNED);
                }
                return;
            }

            MyBoard.UpdateCellState(x, y, result);
        }
    }
}
