using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{


    public class Point2d
    {
        public readonly double X;
        public readonly double Y;

        public static readonly Point2d Origo=new Point2d(0.0,0.0);

        public Point2d(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
        

        public double Distance(Point2d p)
        {
            double dx = p.X - X;
            double dy = p.Y - Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public double SquaredDistance(Point2d p)
        {
            double dx = p.X - X;
            double dy = p.Y - Y;
            return dx * dx + dy * dy;
        }


        public Point2d Polar(double angle, double distance)
        {
            return new Point2d(X + Math.Cos(angle) * distance, Y + Math.Sin(angle) * distance);
        }

        public double Angle(Point2d p)
        {
            double res = Math.Atan2(p.Y - Y, p.X - X);
            if (res < 0.0)
                res += MathUtil.Deg360;
            return res;
        }

        public Point2d GetTransformed(Transform2d t)
        {
            double tx, ty;
            t.Apply(X, Y, out tx, out ty, true);
            return new Point2d(tx, ty);
        }

        public static Vector2d operator -(Point2d a, Point2d b) { return new Vector2d(a.X - b.X, a.Y - b.Y); }
        public static Point2d operator +(Point2d p, Vector2d v) { return new Point2d(p.X + v.X, p.Y + v.Y); }
        public static Point2d operator -(Point2d p, Vector2d v) { return new Point2d(p.X - v.X, p.Y - v.Y); }

        public override string ToString()
        {
            return StringUtil.FormatReals(X, Y);
        }

        public Point2d ClosestPoint(IEnumerable<Point2d> pnts)
        {
            if (pnts == null) 
                return null;

            Point2d res = null;
            double sqbest=0.0;
            foreach(Point2d pt in pnts) {
                double td=SquaredDistance(pt);
                if (res == null || sqbest > td)
                {
                    sqbest = td;
                    res = pt;
                }
            }
            return res;
        }

        public Point2d ClosestPoint(params Point2d[] pnts)
        {
            return ClosestPoint((IEnumerable<Point2d>)pnts);
        }

        public Location Location(Point2d pnt1, Point2d pnt2, double tol=0.0)
        {
            double det1 = (pnt1.X - pnt2.X) * (pnt1.Y - Y);
            double det2 = (pnt1.Y - pnt2.Y) * (pnt1.X - X);

            if (Math.Abs(det1 - det2) <= tol)
                return Guppy2.Calc.Location.On;
            else if (det1 > det2)
                return Guppy2.Calc.Location.Left;
            else
                return Guppy2.Calc.Location.Right;
        }
        
    }
}
