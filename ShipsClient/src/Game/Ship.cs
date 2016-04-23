using System.Windows;
using ShipsClient.Enums;

namespace ShipsClient.Game
{
    public class Ship
    {
        public Ship(int length)
        {
            Length = length;
        }

        public Ship(int length, ShipOrientation orientation, int x, int y, int hitCount)
        {
            Length = length;
            Orientation = orientation;
            X = x;
            Y = y;
            HitCount = hitCount;
        }

        public int Length { get; set; }
        public ShipOrientation Orientation { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int HitCount { get; set; }

        public bool IsDrowned
        {
            get { return HitCount == Length; }
        }

        public bool IsLocatedAt(int x, int y)
        {
            var rect = GetShipRegion();

            return (x >= rect.X && x <= rect.Right && y >= rect.Y && y <= rect.Bottom);
        }

        public Rect GetShipRegion()
        {
            var width = Orientation == ShipOrientation.SHIP_ORIENTATION_HORIZONTAL ? Length : 1;
            var height = Orientation == ShipOrientation.SHIP_ORIENTATION_VERTICAL ? Length : 1;

            return new Rect(X, Y, width, height);
        }

        public bool IsInRegion(Rect rect)
        {
            var r = GetShipRegion();
            return (rect.IntersectsWith(r));
        }

        public void MoveTo(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Rotate()
        {
            Orientation = Orientation == ShipOrientation.SHIP_ORIENTATION_HORIZONTAL
                ? ShipOrientation.SHIP_ORIENTATION_VERTICAL
                : ShipOrientation.SHIP_ORIENTATION_HORIZONTAL;
        }
    }
}
