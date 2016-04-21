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
using System.Windows.Shapes;

namespace ShipsClient.NotificationWindow
{
    /// <summary>
    /// Логика взаимодействия для NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        private string TitleText { get; set; }
        private string NotificationText { get; set; }

        public NotificationWindow(string title, string text)
        {
            InitializeComponent();
            TitleText = title;
            NotificationText = text;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.TitleText = TitleText;
            _lbContent.Text = NotificationText;
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

        private void _btOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
