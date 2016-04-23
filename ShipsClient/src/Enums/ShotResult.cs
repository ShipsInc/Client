namespace ShipsClient.Enums
{
    public enum ShotResult
    {
        SHOT_RESULT_NONE                    = 0,
        SHOT_RESULT_MISSED                  = 1,
        SHOT_RESULT_SHIP_HIT                = 2,
        SHOT_RESULT_SHIP_DROWNED            = 3,
        SHOT_RESULT_YOU_SHOT_IT_CELL        = 4, // Стрельба по уже стрельнутой клетке
        SHOT_RESULT_RESET_CELL              = 5,
    }
}
