using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{
    public class Arc2d : Curve2d
    {
        private Point2d start;
        private Point2d end;
        private double bulge;

        #region CONSTRUCTORS

        public Arc2d(Point2d start, Point2d end, double bulge)
        {
            this.start = start;
            this.end = end;
            this.bulge = bulge;
        }

        public Arc2d(Arc2d tocopy)
        {
            this.start = tocopy.start;
            this.end = tocopy.end;
            this.bulge = tocopy.bulge;
        }

        #endregion

        #region ACCESSORS

        public Point2d Start { get { return start; } set { start = value; } }
        
        public Point2d End { get { return end; } set { end = value; } }

        public double Bulge { get { return bulge; } set { bulge = value; } }

        #endregion


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
            get { return Radius*SignedSweepAngle; }
        }

      
        /// <summary>
        /// Computes parameter of point on arc in range 0.0-1.0
        /// If the point is not on the arc
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public override Point2d PointAt(double t)
        {
            double sa = StartAngle;
            double ea = EndAngle;
            double ptang = sa + (ea - sa) * t;

            if (CW)
                ptang = MathUtil.Deg360 - ptang;
            
            return Center.Polar(ptang,Radius);
        }

        /// <summary>
        /// Returns the starting angle of the arc from
        /// the xaxis and the arcs direction
        /// </summary>
        public double StartAngle
        {
            get
            {
                double res=Center.Angle(start);
                if (CW)
                {
                    res = MathUtil.Deg360 - res;
                    if (res >= MathUtil.Deg360) //for zero angle not to become 2pi
                        res = 0.0;
                }
                return res;
            }
        }

        public double EndAngle
        {
            get
            {
                return StartAngle + SweepAngle;
            }
        }

        public bool IsAngleOnArc(double ang)
        {
            if (Linear) return false;
            
            ang = MathUtil.NormalizeAngle(ang);
            
            if (CCW)
            {
                if (ang < StartAngle) ang += MathUtil.Deg360;
                return ang <= EndAngle;
            }
            else
            {
                if (ang > StartAngle) ang -= MathUtil.Deg360;
                return ang >= EndAngle;
            }
        }


        public bool CCW { get { return bulge > MathUtil.Epsilon; } }
       
        public bool CW { get { return bulge < -MathUtil.Epsilon; } }
      
        public bool Linear { get {return !(CW || CCW);}}

        public override void Reverse()
        {
            Point2d tmp = start;
            start = end;
            end = tmp;
            bulge = -bulge;
        }

        public override double Distance(Point2d p)
        {
            if(IsAngleOnArc(Center.Angle(p)))
                return Center.Distance(p)-Radius;
            else
                return Math.Min(start.Distance(p),end.Distance(p));
        }

        public double SweepAngle { get { return Math.Abs(SignedSweepAngle); } }
      
        public double SignedSweepAngle { get { return 4.0*Math.Atan(bulge); } }
      
        public double Radius { get {return Math.Abs(start.Distance(end) * (bulge + 1.0 / bulge) / 4.0); }}
        
        public Point2d Center
        {
            get
            {
                if (Math.Abs(bulge) < MathUtil.Epsilon)
                    return new Point2d(double.PositiveInfinity, double.PositiveInfinity);
                double fac = ((1.0 / bulge) - bulge) / 2.0;
                return new Point2d(
                    ((start.X + end.X) - (end.Y - start.Y) * fac) / 2.0,
                    ((start.Y + end.Y) + (end.X - start.X) * fac) / 2.0);
            }
        }

        public override bool Transform(Transform2d t)
        {
            if (t.IsUniform)
                return false;

            start = start.GetTransformed(t);
            end = end.GetTransformed(t);

            if (t.Determinant < 0.0)
                bulge = -bulge; //mirror

            return true;
        }


        public Point2d MidPoint
        {
            get
            {
                return new Point2d(
                    0.5 * (bulge * (end.Y - start.Y) + start.X + end.X),
                    0.5 * (bulge * (start.X - end.X) + start.Y + end.Y)
                );
            }
        }

        public override double PositionOf(Point2d pnt)
        {
            if (Linear)
                return new Line2d(start, end).PositionOf(pnt);

            Point2d ce=Center;
            Vector2d org = ce - MidPoint;    //point in to middle of conjugate arc as base for parameter
            double swa = SweepAngle;

            if (swa == 0.0) //zero length arc
                throw new Exception("Param() on zero length arc is not possible");

            double par=(org.AngleCCW(pnt - ce) - org.AngleCCW(start - ce)) / swa;
            if (CCW)
                return par;
            else
                return -par;
        }


        public bool IsCirclePointOnArc(Point2d pnt,double tol=0)
        {
            if (Linear)
                return MathUtil.IsBetween(new Line2d(start, end).PositionOf(pnt), -tol, 1.0 + tol);
            if (CW)
                return pnt.Location(start, end, tol) != Location.Right;
            else
                return pnt.Location(start, end, tol) != Location.Left;

        }

        public override Point2d ClosestPoint(Point2d from)
        {

            if (Linear)
                return new Line2d(start, end).ClosestPoint(from);
            
            Point2d ce=Center;
            Point2d oncirc = ce.Polar(ce.Angle(from), Radius);

            if (IsCirclePointOnArc(oncirc))
                return oncirc;

            return from.ClosestPoint(start, end);
        }

        public override Vector2d Tangent(Point2d where)
        {
            if (Linear)
                return (end - start).Normalized;

            Vector2d v = (where - Center).Normalized;
            if (v == null) return null;

            return CCW ? v.Rotated90 : v.Rotated270;
        }

        public override Vector2d DirectionAt(double t)
        {

            //TODO: try this out
            if (Linear)
                return (end - start).Normalized;

            if (CCW)
                return Vector2d.FromAngle(StartAngle + t * MathUtil.Deg360 + MathUtil.Deg90);
            else
                return Vector2d.FromAngle(StartAngle + t * MathUtil.Deg360 - MathUtil.Deg90);
        }
    }
}
