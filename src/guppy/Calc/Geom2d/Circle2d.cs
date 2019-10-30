using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{



    public class Circle2d : Conic2d
    {
        private Point2d center;
        private double radius;


        #region CONSTRUCTORS

        public Circle2d(Point2d center, double radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public Circle2d(double xcenter, double ycenter, double radius)
        {
            this.center = new Point2d(xcenter, ycenter);
            this.radius = radius;
        }

        public Circle2d(Circle2d tocopy)
        {
            center = tocopy.center;
            radius = tocopy.radius;
        }

        #endregion


        #region ACCESSORS

        public Point2d Center { get { return center; } set { center = value; } }

        public double Radius { get { return radius; } set { radius = value; } }
        
        public double X { get { return center.X; } set { center = new Point2d(value, center.Y); } }
        
        public double Y { get { return center.Y; } set { center = new Point2d(center.X, value); } }

        #endregion


        #region CONVERTERS

        public static Circle2d From2Points(Point2d p1, Point2d p2)
        {
            Point2d center = new Point2d((p1.X + p2.X) / 2.0, (p1.Y + p2.Y) / 2.0);
            return new Circle2d(center,center.Distance(p1));
        }

        public override GeneralConic2d ToGeneralConic()
        {
            return new GeneralConic2d(1, 0, 1, 0, 0, -radius * radius);
        }

        #endregion



        public override double Length { get { return radius * 2.0 * Math.PI; } }

        public override double Area { get { return radius * radius * Math.PI; } }

        public override double Distance(Point2d p) { return Math.Abs(center.Distance(p) - radius); }

        public override bool IsClosed { get { return true; } }

        public override Point2d PointAt(double t) { return center.Polar(t * MathUtil.Deg360, radius); }

        public override void Reverse() { /*TODO: store direction in circle? */ }

        public override bool Transform(Transform2d t)
        {
            if (!t.IsUniform) return false; //circle must still be circle after transform

            radius = new Vector2d(radius, 0.0).GetTransformed(t).Length;
            center = center.GetTransformed(t);

            return true;
        }

        public override Point2d ClosestPoint(Point2d from)
        {
            // Closest point on circle, derived from lagrange multiplier 
            // minimizing (x-i)^2+(y-j)^2 with constraint x^2+y^2-r^2=0
            // computed by Robert Persson

            double i = from.X - center.X; // translate to origo
            double j = from.Y - center.Y;

            if (MathUtil.IsZero(i) && MathUtil.IsZero(j)) //center of circle=>any point will do
                return new Point2d(center.X + radius, center.Y);

            double r2 = radius*radius;
            double f=r2/Math.Sqrt(r2*(i*i+j*j));
            return new Point2d(i * f + center.X, j * f + center.Y); //translate back
        }

        public override double PositionOf(Point2d pnt)
        {
            return center.Angle(pnt) / MathUtil.Deg360; // keep between 0-1
        }

        public override Vector2d Tangent(Point2d where)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return center.ToString() + " " + StringUtil.FormatReal(radius);
        }

        public override Vector2d DirectionAt(double t)
        {
            return Vector2d.FromAngle(t + MathUtil.Deg90);
        }
    }
}
