namespace ShipsClient.Common
{
    public class Constants
    {
        // Информация о соединении
        public static string CONNECT_INFO_HOST = "127.0.0.1";
        public static int CONNECT_INFO_PORT = 8085;

        // Информация о логине
        public static int USERNAME_MIN_LENGHT = 2;
        public static int USERNAME_MAX_LENGHT = 7;

        // Игроковое поле
        public static int CELL_SIZE = 25;
        public static readonly byte BOARD_SIZE = 10;
    }
}
