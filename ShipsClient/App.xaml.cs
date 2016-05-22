using System.Windows;
using ShipsClient.Protocol.Parser;

namespace ShipsClient
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private App()
        {
            Handler.LoadHandlers();
        }
    }
}
