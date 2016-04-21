using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ShipsClient.Enums;

namespace ShipsClient.Game
{
    public class BoardCell : Label
    {
        private BoardCellState _state;

        public BoardCell(int x, int y, BoardCellState state = BoardCellState.Normal)
        {
            X = x;
            Y = y;
            this.State = state;
            base.AllowDrop = true;
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
            switch (_state)
            {
                case BoardCellState.Normal:
                    BorderThickness = new Thickness(0.0f);
                    BorderBrush = new SolidColorBrush(Colors.Transparent);
                    Background = new SolidColorBrush(Colors.Transparent);
                    break;
                case BoardCellState.MissedShot:
                    Background = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/Battle/missed_cell.png")));
                    break;
                case BoardCellState.Ship:
                    BorderThickness = new Thickness(0.0f);
                    BorderBrush = new SolidColorBrush(Colors.Transparent);
                    Background = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Resources/Ships/ship-1-normal.png")));
                    break;
                case BoardCellState.ShotShip:
                    Background = new SolidColorBrush(Colors.Chartreuse);
                    break;
                case BoardCellState.ShipDrag:
                    BorderThickness = new Thickness(1);
                    BorderBrush = new SolidColorBrush(Colors.Green);
                    break;
                case BoardCellState.ShipDragInvalid:
                    BorderThickness = new Thickness(1);
                    BorderBrush = new SolidColorBrush(Colors.Red);
                    break;
                case BoardCellState.ShowDrowned:
                    Background = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    break;
            }
        }

        public int X { get; private set; }
        public int Y { get; private set; }
    }
}
