using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ShipsClient.Common;
using ShipsClient.Enums;
using Size = System.Drawing.Size;

namespace ShipsClient.Game
{
    public class Board : UserControl
    {
        private static readonly Rect BoardRegion = new Rect(0, 0, 10, 10);

        private readonly BoardCell[,] _cells;
        private readonly bool _drawShips;
        private readonly List<Ship> _ships;
        private readonly Random _rnd;
        private Canvas _grid;
        private DraggableShip _draggedShip;

        public Board(Canvas grid, bool drawShips = true)
        {
            _grid = grid;
            _drawShips = drawShips;
            _cells = new BoardCell[Constants.BOARD_SIZE, Constants.BOARD_SIZE];
            _rnd = new Random(DateTime.Now.Millisecond);
            _ships = new List<Ship>();

            CreateBoard();
        }

        public void ReadPacket(Packet packet)
        {
            for (var i = 0; i < Constants.BOARD_SIZE; ++i)
            {
                for (var j = 0; j < Constants.BOARD_SIZE; ++j)
                {
                    var state = packet.ReadUInt8();
                    _cells[i, j] = new BoardCell(i, j, (BoardCellState)state);
                }
            }

            var count = packet.ReadInt32();
            for (var i = 0; i < count; ++i)
            {
                var length = packet.ReadUInt8();
                var orientation = packet.ReadUInt8();
                var x = packet.ReadInt16();
                var y = packet.ReadInt16();
                var hitCount = packet.ReadUInt8();
                _ships.Add(new Ship(length, (ShipOrientation)orientation, x, y, hitCount));
            }
        }

        public void WritePacket(Packet packet)
        {
            for (var i = 0; i < Constants.BOARD_SIZE; ++i)
            {
                for (var j = 0; j < Constants.BOARD_SIZE; ++j)
                    packet.WriteUInt8((byte)_cells[i, j].State);
            }

            packet.WriteInt32(_ships.Count);
            foreach (var ship in _ships)
            {
                packet.WriteUInt8((byte)ship.Length);
                packet.WriteUInt8((byte)ship.Orientation);
                packet.WriteInt16((short)ship.X);
                packet.WriteInt16((short)ship.Y);
                packet.WriteUInt8((byte)ship.HitCount);
            }
        }

        private void CreateBoard()
        {
            var boardSize = new Size(Constants.CELL_SIZE * BoardRegion.Width + Constants.CELL_SIZE, Constants.CELL_SIZE * BoardRegion.Height + Constants.CELL_SIZE);
            base.MinWidth = boardSize.Width;
            base.MinHeight = boardSize.Height;
            base.Width = boardSize.Width;
            base.Height = boardSize.Height;

            var points = BoardRegion.GetPoints();

            using (var d = Dispatcher.DisableProcessing())
            {
                foreach (var point in points)
                {
                    var cell = new BoardCell(point.X, point.Y)
                    {
                        Margin = new Thickness(point.Y*Constants.CELL_SIZE + 3, point.X*Constants.CELL_SIZE + 3, 0, 0),
                        Width = Constants.CELL_SIZE,
                        Height = Constants.CELL_SIZE,
                        State = BoardCellState.Normal
                    };
                    cell.MouseLeftButtonDown += OnCellMouseDown;
                    cell.DragEnter += OnCellDragEnter;
                    cell.DragLeave += OnCellDragLeave;
                    cell.Drop += OnCellDragDrop;
                    cell.QueryContinueDrag += OnCellQueryContinueDrag;
                    cell.MouseLeftButtonDown += OnCellClick;
                    _cells[point.X, point.Y] = cell;
                    _grid.Dispatcher.Invoke(new Action(() =>
                    {
                        _grid.Children.Add(cell);
                    }
                    ));
                }
            }
        }

        private void OnCellClick(object sender, EventArgs e)
        {
            if (Status != BoardStatus.BOARD_STATUS_BATTLE)
                return;

            var handler = OnClick;
            if (handler == null)
                return;

            var cell = (BoardCell)sender;
            var eventArgs = new BoardCellClickEventErgs(cell.X, cell.Y);
            handler(this, eventArgs);
        }

        public Ship GetShipAt(int x, int y)
        {
            return _ships.FirstOrDefault(ship => ship.IsLocatedAt(x, y));
        }

        private void OnCellMouseDown(object sender, MouseEventArgs e)
        {
            if (Status == BoardStatus.BOARD_STATUS_BATTLE || !_drawShips)
                return;

            var cell = (BoardCell)sender;
            var ship = GetShipAt(cell.X, cell.Y);

            if (ship == null)
            {
                return;
            }

            _draggedShip = DraggableShip.From(ship);
            DragDrop.DoDragDrop(cell, ship, DragDropEffects.Copy | DragDropEffects.Move);
        }

        private void OnCellQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            // check Ctrl key state
            bool shouldRotate = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey;
            bool isRotated = _draggedShip.IsOrientationModified;

            if ((shouldRotate && isRotated) || (!shouldRotate && !isRotated))
                return;

            var rect = _draggedShip.GetShipRegion();
            RedrawRegion(rect);

            _draggedShip.Rotate();
            _draggedShip.IsOrientationModified = !isRotated;

            var state = CanPlaceShip(_draggedShip, _draggedShip.X, _draggedShip.Y) ? BoardCellState.ShipDrag : BoardCellState.ShipDragInvalid;
            DrawShip(_draggedShip, state);
        }

        private void OnCellDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Ship)))
            {
                var cell = (BoardCell)sender;
                _draggedShip.MoveTo(cell.X, cell.Y);

                var canPlaceShip = CanPlaceShip(_draggedShip, cell.X, cell.Y);
                var state = canPlaceShip ? BoardCellState.ShipDrag : BoardCellState.ShipDragInvalid;

                DrawShip(_draggedShip, state);

                e.Effects = canPlaceShip ? DragDropEffects.Move : DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void OnCellDragLeave(object sender, EventArgs e)
        {
            var rect = _draggedShip.GetShipRegion();
            RedrawRegion(rect);
        }

        private void OnCellDragDrop(object sender, DragEventArgs e)
        {
            var cell = (BoardCell)sender;
            if (e.Data.GetDataPresent(typeof(Ship)))
            {
                if (!CanPlaceShip(_draggedShip, cell.X, cell.Y))
                {
                    RedrawRegion(_draggedShip.GetShipRegion());
                    return;
                }

                var ship = _draggedShip.Source;
                _ships.Remove(ship);

                var rect = ship.GetShipRegion();
                RedrawRegion(rect);

                ship.Orientation = _draggedShip.Orientation;

                AddShip(ship, cell.X, cell.Y);
                _draggedShip = null;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        public void AddShip(Ship ship, int x, int y)
        {
            ship.MoveTo(x, y);

            _ships.Add(ship);
            DrawShip(ship, BoardCellState.Ship);
        }

        private bool CanPlaceShip(Ship ship, int x, int y)
        {
            var shipRegion = ship.GetShipRegion();

            shipRegion.MoveTo(x, y);

            if (!BoardRegion.Contains(shipRegion))
                return false;

            shipRegion.Inflate(1, 1);

            foreach (var s in _ships)
            {
                if (ship is DraggableShip && s == ((DraggableShip)ship).Source)
                {
                    continue;
                }

                if (s.GetShipRegion().IntersectsWith(shipRegion))
                {
                    return false;
                }
            }
            return true;
        }

        private void RedrawRegion(Rect region)
        {
            using (var d = Dispatcher.DisableProcessing())
            {
                var points = region.GetPoints();
                foreach (var point in points)
                {
                    if (!BoardRegion.Contains(point))
                    {
                        continue;
                    }

                    var ship = GetShipAt(point.X, point.Y);
                    _cells[point.X, point.Y].Dispatcher.Invoke(new Action(() =>
                    {
                        _cells[point.X, point.Y].State = ship == null ? BoardCellState.Normal : BoardCellState.Ship;
                    }));
                }
            }
        }

        private void DrawShip(Ship ship, BoardCellState state)
        {
            DrawShip(ship, state, false);
        }

        private void DrawShip(Ship ship, BoardCellState state, bool force)
        {
            if (!_drawShips && !force)
                return;

            var points = ship.GetShipRegion().GetPoints();

            using (var d = Dispatcher.DisableProcessing())
            {
                foreach (var point in points)
                {
                    if (BoardRegion.Contains(point))
                    {
                        _cells[point.X, point.Y].Dispatcher.Invoke(new Action(() =>
                        {
                            _cells[point.X, point.Y].State = state;
                        }));
                    }
                }
            }
        }

        public void ClearBoard()
        {
            _ships.Clear();

            var points = BoardRegion.GetPoints();
            foreach (var point in points)
            {
                _cells[point.X, point.Y].State = BoardCellState.Normal;
            }
        }

        public void AddRandomShips()
        {
            ClearBoard();

            var ships = GetNewShips();

            foreach (var ship in ships)
            {
                var shipAdded = false;

                while (!shipAdded)
                {
                    var x = _rnd.Next(10);
                    var y = _rnd.Next(10);

                    if (!CanPlaceShip(ship, x, y))
                        continue;

                    AddShip(ship, x, y);
                    shipAdded = true;
                }
            }

        }

        private IList<Ship> GetNewShips()
        {
            var ships = new List<Ship>
                        {
                            new Ship(4){Orientation = (ShipOrientation)_rnd.Next(2)},
                            new Ship(3){Orientation = (ShipOrientation)_rnd.Next(2)},
                            new Ship(3){Orientation = (ShipOrientation)_rnd.Next(2)},
                            new Ship(2){Orientation = (ShipOrientation)_rnd.Next(2)},
                            new Ship(2){Orientation = (ShipOrientation)_rnd.Next(2)},
                            new Ship(2){Orientation = (ShipOrientation)_rnd.Next(2)},
                            new Ship(1){Orientation = (ShipOrientation)_rnd.Next(2)},
                            new Ship(1){Orientation = (ShipOrientation)_rnd.Next(2)},
                            new Ship(1){Orientation = (ShipOrientation)_rnd.Next(2)},
                            new Ship(1){Orientation = (ShipOrientation)_rnd.Next(2)}
                        };

            return ships;
        }

        public void SetGrid(Canvas grid)
        {
            _grid.Children.Clear();
            grid.Children.Clear();
            _grid = grid;

            var points = BoardRegion.GetPoints();
            using (var d = Dispatcher.DisableProcessing())
            {
                _grid.Dispatcher.Invoke(new Action(() =>
                {
                    foreach (var point in points)
                        _grid.Children.Add(_cells[point.X, point.Y]);
                }));
            }
        }

        public void Refresh()
        {
            RedrawRegion(BoardRegion);
        }

        public BoardStatus Status { get; set; }

        public void UpdateCellState(int x, int y, ShotResult result)
        {
            BoardCellState bState = BoardCellState.Normal;
            switch (result)
            {
                case ShotResult.SHOT_RESULT_MISSED:
                    bState = BoardCellState.MissedShot;
                    break;
                case ShotResult.SHOT_RESULT_SHIP_HIT:
                    bState = BoardCellState.ShotShip;
                    break;
                case ShotResult.SHOT_RESULT_SHIP_DROWNED:
                    bState = BoardCellState.ShowDrowned;
                    break;
                default:
                    throw new NotSupportedException($"UpdateCellState not supported result {result}");
            }

            _cells[x, y].State = bState;
        }

        public new event EventHandler<BoardCellClickEventErgs> OnClick;
    }
}
