namespace ShipsClient.Common
{
    public class Constants
    {
        // Информация о соединении
        public static readonly string CONNECT_INFO_HOST                 = "37.46.134.95";
        //public static readonly string CONNECT_INFO_HOST                 = "127.0.0.1";
        public static readonly int CONNECT_INFO_PORT                    = 8085;

        // Общая
        public static readonly int BUFFER_SIZE                          = 256;

        // Информация о логине
        public static readonly int USERNAME_MIN_LENGHT                  = 2;
        public static readonly int USERNAME_MAX_LENGHT                  = 7;

        // Игроковое поле
        public static readonly int CELL_SIZE                            = 25;
        public static readonly byte BOARD_SIZE                          = 10;

        // Криптография
        public static readonly string CRYPTOGRAPHY_PASSWORD             = "13W_1q_ew3e$%213ASe";
    }
}
