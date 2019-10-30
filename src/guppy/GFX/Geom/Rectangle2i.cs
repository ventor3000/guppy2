using Guppy2.Calc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GFX
{
    public class Rectangle2i
    {
        public readonly int XMin, YMin, XMax, YMax;

        public static readonly Rectangle2i Empty = new Rectangle2i(0, 0, 0, 0); //the one and only empty rectangle



        public Rectangle2i(int x1, int y1, int x2, int y2)
        {
            MathUtil.Sort(ref x1, ref x2);
            MathUtil.Sort(ref y1, ref y2);
            XMin = x1;
            YMin = y1;
            XMax = x2;
            YMax = y2;
        }


        public int Width { get { return XMax - XMin; } }
        public int Height { get { return YMax - YMin; } }
        public int Area { get { return Width * Height; } }
        public bool IsEmpty { get { return this == Empty; } }

        public Rectangle2i Append(int x, int y)
        {
            if (IsEmpty) return new Rectangle2i(x, y, x, y);
            return new Rectangle2i(Math.Min(x, XMin), Math.Min(y, YMin), Math.Max(x, XMax), Math.Max(y, YMax));
        }


        public Rectangle2i Append(Rectangle2i rect)
        {
            if (rect.IsEmpty) return this;
            else if (IsEmpty) return rect;
            else return new Rectangle2i(Math.Min(XMin, rect.XMin), Math.Min(YMin, rect.YMin), Math.Max(XMax, rect.XMax), Math.Max(YMax, rect.YMax));
        }

        public override string ToString()
        {
            return XMin.ToString() + "," + YMin.ToString() + "," + XMax.ToString() + "," + YMax.ToString();
        }

        public bool Overlaps(Rectangle2i r)
        {
            if (r.XMin > XMax || r.XMax < XMin || r.YMax < YMin || r.YMin > YMax)
                return false;
            return true;
        }

        public bool Contains(Point2i pt)
        {
            if (pt.X < XMin || pt.Y < YMin || pt.X > XMax || pt.Y > YMax)
                return false;
            return true;
        }
    }
}
