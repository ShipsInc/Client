namespace ShipsClient.Enums
{
    public enum BoardCellState
    {
        BOARD_CELL_STATE_NORMAL                         = 0,
        BOARD_CELL_STATE_MISSED_SHOT                    = 1,
        BOARD_CELL_STATE_SHIP                           = 2,
        BOARD_CELL_STATE_SHOT_SHIP                      = 3,
        BOARD_CELL_STATE_SHIP_DRAG                      = 4,
        BOARD_CELL_STATE_SHIP_DRAG_INVALID              = 5,
        BOARD_CELL_STATE_SHOW_DROWNED                   = 6,
    }
}
