using ShipsClient.Common;
using ShipsClient.Network;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ShipsClient.Messages;
using ShipsClient.Protocol;
using ShipsClient.Protocol.Parser;

namespace ShipsClient.Auth
{
    /// <summary>
    /// Логика взаимодействия для Auth.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private static AuthWindow _instance;
        private static readonly object LockObj = new object();
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
                lock (LockObj)
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
            Task.Factory.StartNew(() => { TCPSocket.Instance.Connect(Constants.CONNECT_INFO_HOST, Constants.CONNECT_INFO_PORT); });
        }

        private void _btClose_Click(object sender, RoutedEventArgs e)
        {
            if (TCPSocket.Instance.IsOpen())
                TCPSocket.Instance.SendPacket(new Packet(Opcode.CMSG_DISCONNECTED));

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
            var packet = new Packet(Opcode.CMSG_AUTH);
            packet.WriteUTF8String(_tbUsername.Text);
            packet.WriteUTF8String(_tbPassword.Password);
            TCPSocket.Instance.SendPacket(packet);
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

        private void _tbUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_tbUsername.Text.Equals("Логин", StringComparison.OrdinalIgnoreCase))
                _tbUsername.Text = string.Empty;
        }

        private void _tbUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_tbUsername.Text))
                _tbUsername.Text = "Логин";
        }

        private void _tbPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_tbPassword.Password.Equals("******", StringComparison.OrdinalIgnoreCase))
                _tbPassword.Password = string.Empty;
        }

        private void _tbPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_tbPassword.Password))
                _tbPassword.Password = "******";
        }

        private void Rectangle_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _tbUsername_LostFocus(null, null);
            _tbPassword_LostFocus(null, null);
        }
    }
}
