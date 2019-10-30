using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{

     

    /// <summary>
    /// A line segment, stored as start and end point.
    /// </summary>
    public class Line2d:Curve2d
    {
        private Point2d start;
        private Point2d end;

        #region CONSTRUCTORS

        public Line2d(Point2d start, Point2d end)
        {
            this.start = start;
            this.end = end;
        }

        public Line2d(double x1, double y1, double x2, double y2)
        {
            this.start = new Point2d(x1, y1);
            this.end = new Point2d(x2, y2);
        }

        public Line2d(Line2d tocopy)
        {
            start = tocopy.start;
            end = tocopy.end;
        }

        #endregion

        #region ACCESSORS

        public double X1 { get { return start.X; } }

        public double Y1 { get { return start.Y; } }
        
        public double X2 { get { return end.X; } }
        
        public double Y2 { get { return end.Y; } }

        public Point2d Start { get { return start; } set { start = value; } }
        
        public Point2d End { get { return end; } set { end = value; } }

        public double DX { get { return end.X - start.X; } }
        
        public double DY { get { return end.Y - start.Y; } }

        #endregion


        #region CONVERTERS
        
        public static Line2d FromEquation(double a, double b, double c)
        {
            Point2d start, end;
            if (MathUtil.IsZero(b))
                start = new Point2d(-c / a, 1);
            else
                start = new Point2d(0, -c / b);

            if (MathUtil.IsZero(a))
                end = new Point2d(1.0, -c / b);
            else if (MathUtil.IsZero(c))
                end = new Point2d(b, -a);
            else
                end = new Point2d(-c / a, 0);

            return new Line2d(start, end);
        }

        /// <summary>
        /// Computes the coefficients for the line int the form ax+by+c=0
        /// The coefficients are normalized. Returns true on success or false
        /// if the line has zero length.
        /// </summary>
        public bool ToEquation(out double a, out double b, out double c)
        {
            // Get constants
            double xdiff = end.X - start.X;
            double ydiff = end.Y - start.Y;
            double rsquare = (xdiff * xdiff) + (ydiff * ydiff);
            double rinv = 1.0 / rsquare;


            // derive parameters
            a = -ydiff * rinv;
            b = xdiff * rinv;
            c = (start.X * end.Y - end.X * start.Y) * rinv;

            if (double.IsInfinity(rinv))    //not computable, a,b,c will be NaN
                return false;


            // normalize the equation for convenience
            double sMult = 1.0 / Math.Sqrt(a * a + b * b);
            a *= sMult;
            b *= sMult;
            c *= sMult;

            return true;
        }

        public Arc2d ToArc()
        {
            return new Arc2d(start, end, 0.0);
        }
       

        #endregion

        public override Point2d PointAt(double t)
        {
            return new Point2d(
                start.X * (1.0 - t) + t * end.X,
                start.Y * (1.0 - t) + t * end.Y);
        }

        public override bool IsClosed
        {
            get { return false; }
        }

        public override double Area
        {
            get { return 0.0; }
        }

        public override double Length
        {
            get { return start.Distance(end); }
        }

        
        public override void Reverse()
        {
            var tmp = start;
            start = end;
            end = tmp;
        }

        public override double Distance(Point2d p)
        {
            return ClosestPoint(p).Distance(p);
        }

        /// <summary>
        /// Returns the distance to this linear object as if it was a infinite line.
        /// </summary>
        public double DistanceLine(Point2d p)
        {
            return PointAt(PositionOf(p)).Distance(p); //TODO: maybe speed up a little / lower level
        }

        public override bool Transform(Transform2d t)
        {
            start = start.GetTransformed(t);
            end = end.GetTransformed(t);
            return true;    //a line can always be transformed
        }
        

        public Point2d MidPoint {
            get { return new Point2d((start.X + end.X) * 0.5, (start.Y + end.Y) * 0.5); }
        }

        public override double PositionOf(Point2d pnt)
        {
            double vpx = pnt.X - start.X; //point vector
            double vpy = pnt.Y - start.Y;
            double vlx = end.X - start.X;   //line vector
            double vly = end.Y - start.Y;

            double t=((vpx * vlx) + (vpy * vly)) / (vlx * vlx + vly * vly);
            if (double.IsNaN(t)) //zero length line
                return double.PositiveInfinity;
            return t;
        }

        public override Point2d ClosestPoint(Point2d p)
        {
            return PointAt(MathUtil.Clamp(PositionOf(p),0.0,1.0));  //note that param in line case works for out-of-line points
        }

        public Point2d ClosestPointInfinite(Point2d p)
        {
            return PointAt(PositionOf(p));  //note that param in line case works for out-of-line points
        }


        public override Vector2d Tangent(Point2d pos)
        {
            //easy enough for a line
            return new Vector2d(end.X - start.X, end.Y - start.Y).Normalized;
        }

        
        public double Angle { get { return start.Angle(end); } }


        public override Vector2d DirectionAt(double t) {return (end - start).Normalized;}
        public Vector2d Direction { get { return DirectionAt(0); } }

        public override string ToString()
        {
            return start.ToString() + " " + end.ToString();
        }
    }
}
