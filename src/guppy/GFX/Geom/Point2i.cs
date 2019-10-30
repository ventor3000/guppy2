using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GFX
{ //TODO: this class should probably not be here
    public class Point2i
    {
        public static readonly Point2i Origin = new Point2i(0, 0);

        public readonly int X, Y;

        public Point2i(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public override string ToString()
        {
            return X.ToString() + "," + Y.ToString();
        }

        /// <summary>
        /// Measures 'how much' this point is to the left or right of the line from p0 to p1.
        /// Negative return value => point is right of line
        /// Posetive return value => point is left of line
        /// Zero return value => point is on line
        /// </summary>
        public int SideMeasure(Point2i p0, Point2i p1)
        {
            return SideMeasure(p0, p1, X, Y);
        }

        /// <summary>
        /// Measures 'how much' a point (pointx,pointy) is to the left or right of the line from p0 to p1.
        /// Negative return value => point is right of line
        /// Posetive return value => point is left of line
        /// Zero return value => point is on line
        /// </summary>
        public static int SideMeasure(Point2i p0, Point2i p1, int pointx, int pointy)
        {
            return (p0.X - p1.X) * (p0.Y - pointy) - (p0.Y - p1.Y) * (p0.X - pointx);
        }
    }
}
