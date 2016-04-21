using System;
using System.Windows;
using System.Windows.Input;
using ShipsClient.Common;
using ShipsClient.Messages;
using ShipsClient.Network;

namespace ShipsClient.Auth
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        public static RegistrationWindow Form;

        public RegistrationWindow()
        {
            InitializeComponent();
            Form = this;
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

        public bool RegButtonEnable
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
            { }
        }

        private void _btClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Owner.Visibility = Visibility.Visible;
        }

        private void _btMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void _btPlay_Click(object sender, RoutedEventArgs e)
        {
            int errCode = -1;
            if (_tbUsername.Text.Length == 0 || _tbPassword.Password.Length == 0 || _tbPassword2.Password.Length == 0)
                errCode = 3;
            else if (!_tbPassword.Password.Equals(_tbPassword2.Password))
                errCode = 4;
            else if (_tbUsername.Text.Length > Constants.USERNAME_MAX_LENGHT)
                errCode = 5;
            else if (_tbUsername.Text.Length < Constants.USERNAME_MIN_LENGHT)
                errCode = 6;

            if (errCode != -1)
            {
                _tbErrors.Text = RegMessages.ErrorMessages[errCode];
                return;
            }

            _tbErrors.Text = "";
            Packet packet = new Packet((int)Opcodes.CMSG_REGISTRATION);
            packet.WriteUTF8String(_tbUsername.Text);
            packet.WriteUTF8String(_tbPassword.Password);
            ClientSocket.Instance.SendPacket(packet);
        }
    }
}
