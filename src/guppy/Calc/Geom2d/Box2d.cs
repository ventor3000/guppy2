using Guppy2.AppUtils;
using Guppy2.Calc.Geom2d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.Calc.Geom2d
{
    public class Box2d
    {
        
        public double XMin, YMin, XMax, YMax;
        public bool Empty;

        //public static readonly Rectangle2 Empty = new Rectangle2(0,0,0,0); //the one and only empty rectangle

        public Box2d(double x1, double y1, double x2, double y2)
        {
            MathUtil.Sort(ref x1, ref x2);
            MathUtil.Sort(ref y1, ref y2);
            
            XMin = x1;
            YMin = y1;
            XMax = x2;
            YMax = y2;
            Empty = false;
        }

        public Box2d()
        {
            Empty = true;
        }

        
        
        public double Width { get { return XMax - XMin; } }
        public double Height { get { return YMax - YMin; } }
        public double Area { get { return Width * Height; } }
        public double CenterX { get { return (XMin + XMax) / 2.0; } }
        public double CenterY { get { return (YMin + YMax) / 2.0; } }
        public Point2d Center { get { return new Point2d(CenterX, CenterY); } }
        public Point2d CornerMin { get { return new Point2d(XMin, YMin); } }
        public Point2d CornerMax { get { return new Point2d(XMax, YMax); } }
        
        public void Append(double x, double y)
        {
            if (Empty)
            {
                XMin = XMax = x;
                YMin = YMax = y;
                Empty = false;
            }
            else
            {
                XMin = Math.Min(XMin, x);
                YMin = Math.Min(YMin, y);
                XMax = Math.Max(XMax, x);
                YMax = Math.Max(YMax, y);
            }
        }

        public void Append(Point2d p)
        {
            Append(p.X, p.Y);
        }

        public void Append(double x1, double y1, double x2, double y2)
        {
            Append(x1, y1);
            Append(x2, y2);
        }

        public void Append(Box2d rect)
        {
            Append(rect.XMin, rect.YMin, rect.XMax, rect.YMax);
        }

        public void Scale(double scale, Point2d orig)
        {
            XMin = (XMin - orig.X) * scale + orig.X;
            YMin = (YMin - orig.Y) * scale + orig.Y;
            XMax = (XMax - orig.X) * scale + orig.X;
            YMax = (YMax - orig.Y) * scale + orig.Y;
        }

        public override string ToString()
        {
            if (Empty)
                return "<empty>";
            return Conv.FloatsToStr(XMin, YMin, XMax, YMax);
        }

        public Box2d Offset(double dist)
        {
            return new Box2d(XMin - dist, YMin - dist, XMax + dist, YMax + dist);
        }

        public bool Contains(double x, double y)
        {
            if (x < XMin || x > XMax || y < YMin || y > YMax)
                return false;
            return true;
        }

        public bool Contains(Point2d p)
        {
            return Contains(p.X, p.Y);
        }

    }
}
