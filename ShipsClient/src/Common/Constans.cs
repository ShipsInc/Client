namespace ShipsClient.Common
{
    public class Constants
    {
        // Информация о соединении
        public static readonly string CONNECT_INFO_HOST                 = "10.31.175.111";
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
        public static readonly byte[] CRYPTOGRAPHY_BYTES                = new byte[] { 0x43, 0x87, 0x23, 0x72 };
    }
}
