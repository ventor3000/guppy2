using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{
    public class Parabola2d:Conic2d
    {
        Point2d vertex;
        double rotation;
        double a;  

        // based on  y = a( x – h)^2 + k where h,k is vertex pos and focal distance is 1/(4a)

        public Parabola2d(Point2d vertex, double angle, double focaldist)
        {

            if (focaldist == 0.0)
                throw new Exception("Invalid parameters for parabola => focal distance cannot be 0.0");

            this.vertex = vertex;
            this.rotation = angle;
            this.a = 1.0/(4.0 * focaldist);
        }

        public Parabola2d(Parabola2d tocopy)
        {
            vertex = tocopy.vertex;
            rotation = tocopy.rotation;
            a = tocopy.a;
        }


        #region CONVERTERS


        public static Parabola2d FromFocusAndDirectrix(Point2d focus, Line2d directrix)
        {
            Point2d ondir=directrix.ClosestPointInfinite(focus);
            Point2d vertex = new Point2d(0.5 * (ondir.X + focus.X), 0.5 * (ondir.Y + focus.Y));
            double focdist = focus.Distance(vertex);
            double rot = focus.Angle(ondir) + MathUtil.Deg90;


            if (focdist <= 1e-30)
                return null;
            
            return new Parabola2d(vertex, rot, focdist);
        }


        public override GeneralConic2d ToGeneralConic()
        {

            Transform2d tr = Transform2d.Rotate(rotation) * Transform2d.Translate(vertex.X, vertex.Y);
            GeneralConic2d res = new GeneralConic2d(a, 0, 0, 0, -1, 0); //y=ax^2
            res.Transform(tr);
            return res; //TODO: optimize this function
        }

        #endregion

        #region ACCESSORS

        public Point2d Vertex { get { return vertex; } set { vertex = value; } }
        public double Rotation { get { return rotation; } set { rotation = value; } }
        public Point2d Focus { get { return vertex.Polar(rotation + MathUtil.Deg90, FocalDistance); } }
        public double FocalDistance { get { return 1.0/(4.0*a); } }
        public double A { get { return a; } }

        #endregion


        public Transform2d ToStandardPosition
        {
            get
            {
                //standard position for a parabola has vertex at origo and focal distance of 4.0 (a=0.25)
                //which means the equation of parabola is y=x^2
                if (a == 0.0) return null;    //degenerate
                return Transform2d.Translate(-vertex.X, -vertex.Y) * Transform2d.Rotate(-rotation) * Transform2d.Scale(a);
            }
        }

        public override bool IsClosed
        {
            get { return false; }
        }

        public override double Area
        {
            get { return double.PositiveInfinity; }
        }

        public override double Length
        {
            get { return double.PositiveInfinity; }
        }

       
        public bool Contains(Point2d pt)
        {
            Transform2d tr = ToStandardPosition;
            double i, j;
            tr.Apply(pt.X, pt.Y, out i, out j, true);

            double dy = i * i - j; //y of parabola in std. pos subtracted with point y
            return dy <= 0.0;
        }

        public override Point2d PointAt(double t)
        {
            double f = FocalDistance;
            Point2d res=new Point2d(t,a*t*t);

            //TODO: need to be optimized!!
            return res.GetTransformed(Transform2d.Rotate(rotation) * Transform2d.Translate(vertex.X, vertex.Y));
        }

        public override void Reverse()
        {
            //TODO: should we support direction of parabola?
        }

        public override Vector2d Tangent(Point2d where)
        {
            Transform2d tr = Transform2d.Rotate(-rotation); // ToStandardPosition;
            where = where.GetTransformed(tr);
            Point2d stdvertex = vertex.GetTransformed(tr);

            double slope =  2 * a*(where.X - stdvertex.X); ///(4*FocalDistance); //from diffrentiation

            //TODO: reverse vector if wrong direction
            return Vector2d.FromSlope(slope).GetTransformed(tr.Inversed);
        }

        public override Point2d ClosestPoint(Point2d from)
        {
            return from.ClosestPoint(Perpendicular(from));
        }


        public Point2d[] Perpendicular(Point2d from)
        {
            double i, j; //i,j is from-point in standard space
            Transform2d tr = ToStandardPosition;
            tr.Apply(from.X, from.Y, out i, out j, true);

            //cubic coefficients gotten from langarnge multiplier minimizing distance from i,j to curve
            double[] xs = RealPolynomial.SolveCubic2(-1, 0.0, j-0.25, 0.25*i);
            if (xs == null) return null;

            Point2d[] res = new Point2d[xs.Length];
            tr = tr.Inversed;
            int respos = 0;
            foreach (double x in xs)
            {
                tr.Apply(x, x*x, out i, out j, true);
                res[respos++] = new Point2d(i, j);
            }
            
            return res;
        }


        public override double PositionOf(Point2d pnt)
        {
            //parameter is same as parameter on line in angle of parabola
            return new Line2d(vertex, vertex + Vector2d.FromAngle(rotation)).PositionOf(pnt);
        }

        public override double Distance(Point2d p)
        {
            return ClosestPoint(p).Distance(p);
        }

        public override Vector2d DirectionAt(double t)
        {
            throw new NotImplementedException();
        }

        public override bool Transform(Transform2d t)
        {

            
            //TODO: only manages uniform transformations. Fix this.
            
            Vector2d rv=Vector2d.FromAngleAndLength(rotation,FocalDistance);
            rv=rv.GetTransformed(t);
            rotation = rv.Angle;
            vertex = vertex.GetTransformed(t);
            a = 1.0 / (4 * rv.Length);
            return true;
        }


        #region IConic2d

        

        #endregion
    }
}
