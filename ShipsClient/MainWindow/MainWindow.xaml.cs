﻿using System;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using ShipsClient.BattleWindow;
using ShipsClient.Common;
using ShipsClient.Network;
using ShipsClient.Protocol;

namespace ShipsClient.MainWindow
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Form;

        private System.Timers.Timer KeepAliveTimer = null;

        public object ProfileButton
        {
            set { _btProfile.Content = value; }
            get { return _btProfile.Content; }
        }

        public MainWindow()
        {
            InitializeComponent();
            Form = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TCPSocket.Instance.SendPacket(new Packet(Opcode.CMSG_LOGOUT));

            KeepAliveTimer.Enabled = false;
            Owner.Visibility = Visibility.Visible;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TCPSocket.Instance.SendPacket(new Packet(Opcode.CMSG_PROFILE));

            KeepAliveTimer = new Timer(30000) {Enabled = true};
            KeepAliveTimer.Elapsed += new ElapsedEventHandler(KeepAlive);
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
            Close();
        }

        private void KeepAlive(object source, ElapsedEventArgs e)
        {
            Packet packet = new Packet(Opcode.CMSG_KEEP_ALIVE);
            TCPSocket.Instance.SendPacket(packet);
        }

        private void _btCreateGame_Click(object sender, RoutedEventArgs e)
        {
            var window = new PreBattleWindow { Owner = this };
            window.Show();

            this.Visibility = Visibility.Hidden;
        }

        private void _btJoinGame_Click(object sender, RoutedEventArgs e)
        {
            var packet = new Packet(Opcode.CMSG_GET_GAMES);
            TCPSocket.Instance.SendPacket(packet);
        }

        public void EmptyGames()
        {
            var window = new NotificationWindow.DialogWindow("Создание игры", "Нету игр к которым вы могли бы присоединиться... Желаете создать сами?");
            if (window.ShowDialog() == false)
                return;

            _btCreateGame_Click(null, null);
        }

        public void JoinBattle(int battleId)
        {
            var window = new PreBattleWindow(battleId) { Owner = this };
            window.Show();

            this.Visibility = Visibility.Hidden;
        }

        private void _btProfile_Click(object sender, RoutedEventArgs e)
        {
            TCPSocket.Instance.SendPacket(new Packet(Opcode.CMSG_GET_STATISTICS));
        }
    }
}
