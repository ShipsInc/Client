using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ShipsClient.Common;
using ShipsClient.Enums;

namespace ShipsClient.Game
{
    public class BoardCell : Label
    {
        private BoardCellState _state;
        private Board _board;

        private const char ShipHitChar = '×';

        public BoardCell(Board board, int x, int y, BoardCellState state = BoardCellState.BOARD_CELL_STATE_NORMAL)
        {
            X = x;
            Y = y;
            this.State = state;
            base.AllowDrop = true;
            base.Foreground = new SolidColorBrush(Colors.Red);
            base.VerticalContentAlignment = VerticalAlignment.Center;
            base.HorizontalContentAlignment = HorizontalAlignment.Center;
            base.Padding = new Thickness(-10, -10, -10, -10);
            base.FontSize = 21;

            _board = board;
        }

        public BoardCellState State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                OnCellStateChenged();
            }
        }

        private void OnCellStateChenged()
        {
            Ship ship;
            switch (_state)
            {
                case BoardCellState.BOARD_CELL_STATE_NORMAL:
                    BorderThickness = new Thickness(0.0f);
                    BorderBrush = new SolidColorBrush(Colors.Transparent);
                    Background = new SolidColorBrush(Colors.Transparent);
                    Content = string.Empty;
                    break;
                case BoardCellState.BOARD_CELL_STATE_MISSED_SHOT:
                    Background = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/Battle/missed_cell.png")));
                    break;
                case BoardCellState.BOARD_CELL_STATE_SHIP:
                    BorderThickness = new Thickness(0.0f);
                    BorderBrush = new SolidColorBrush(Colors.Transparent);
                    Width = Constants.CELL_SIZE;
                    Height = Constants.CELL_SIZE;
                    Padding = new Thickness();
                    Content = string.Empty;

                    ship = _board.GetShipAt(X, Y);
                    if (ship != null && ship.X == X && ship.Y == Y)
                    {
                        Background = new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/Resources/Ships/ship-{ship.Length}-normal-{(int)(ship.Orientation == ShipOrientation.SHIP_ORIENTATION_HORIZONTAL ? 1 : 0)}.png")));
                        if (ship.Orientation == ShipOrientation.SHIP_ORIENTATION_HORIZONTAL)
                        {
                            Height = Constants.CELL_SIZE * ship.Length;
                            if (ship.Length > 1)
                                Padding = new Thickness(0, Height - Constants.CELL_SIZE, 0, 0);
                        }
                        else
                        {
                            Width = Constants.CELL_SIZE * ship.Length;
                            if (ship.Length > 1)
                                Padding = new Thickness(0, 0, Width - Constants.CELL_SIZE, 0);
                        }
                    }
                    break;
                case BoardCellState.BOARD_CELL_STATE_SHOT_SHIP:
                    Content = ShipHitChar.ToString();
                    break;
                case BoardCellState.BOARD_CELL_STATE_SHIP_DRAG:
                    BorderThickness = new Thickness(1);
                    BorderBrush = new SolidColorBrush(Colors.Green);
                    break;
                case BoardCellState.BOARD_CELL_STATE_SHIP_DRAG_INVALID:
                    BorderThickness = new Thickness(1);
                    BorderBrush = new SolidColorBrush(Colors.Red);
                    break;
                case BoardCellState.BOARD_CELL_STATE_SHOW_DROWNED:
                {
                    Content = string.Empty;
                    ship = _board.GetShipAt(X, Y);
                    if (ship != null && ship.X == X && ship.Y == Y)
                    {
                        Background = new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/Resources/Ships/ship-{ship.Length}-crash-{(int)(ship.Orientation == ShipOrientation.SHIP_ORIENTATION_HORIZONTAL ? 1 : 0)}.png")));
                        if (ship.Orientation == ShipOrientation.SHIP_ORIENTATION_HORIZONTAL)
                            Height = Constants.CELL_SIZE * ship.Length;
                        else
                            Width = Constants.CELL_SIZE * ship.Length;
                    }
                    break;
                }
                default:
                    break;
            }
        }

        public int X { get; private set; }
        public int Y { get; private set; }
    }
}
