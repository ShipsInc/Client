using ShipsClient.Common;
using ShipsClient.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using ShipsClient.Messages;

namespace ShipsClient.Auth
{
    /// <summary>
    /// Логика взаимодействия для Auth.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private static AuthWindow _instance = null;
        private static readonly object lockObj = new object();
        private static BackgroundWorker backgroundWorker = new BackgroundWorker();

        public static AuthWindow Form;

        public AuthWindow()
        {
            InitializeComponent();
            Form = this;
        }

        public static AuthWindow Instance
        {
            get
            {
                lock (lockObj)
                {
                    if (_instance == null)
                        _instance = new AuthWindow();
                    return _instance;
                }
            }
        }

        public string ErrorMessage
        {
            set
            {
                _tbErrors.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _tbErrors.Text = value;
                }));
            }
            get { return _tbErrors.Text; }
        }

        public bool AuthButtonEnable
        {
            set
            {
                _btPlay.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _btPlay.IsEnabled = value;
                }));
            }
            get { return _btPlay.IsEnabled; }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() => { ClientSocket.Instance.Connect("127.0.0.1", 8085); });
        }

        private void _btClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void _btMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void _btAuth_Click(object sender, RoutedEventArgs e)
        {
            int errCode = -1;
            if (_tbUsername.Text.Length == 0 || _tbPassword.Password.Length == 0)
                errCode = 0;

            if (errCode != -1)
            {
                _tbErrors.Text = AuthMessages.ErrorMessages[errCode];
                return;
            }

            _btPlay.IsEnabled = false;
            _tbErrors.Text = "";
            var packet = new Packet((int)Opcodes.CMSG_AUTH);
            packet.WriteUTF8String(_tbUsername.Text);
            packet.WriteUTF8String(_tbPassword.Password);
            ClientSocket.Instance.SendPacket(packet);
        }

        private void _btJoin_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            var window = new RegistrationWindow {Owner = this};
            window.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        public void ResetControls()
        {
            _tbUsername.Dispatcher.BeginInvoke(new Action(() =>
            {
                _tbUsername.Text = "";
            }));

            _tbPassword.Dispatcher.BeginInvoke(new Action(() =>
            {
                _tbPassword.Password = "";
            }));
        }
    }
}
