using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{
    public class Ellipse2d : Conic2d
    {
        private Point2d center;
        private Vector2d majoraxis;
        private double sigratio;   //signed length ratio for minor axis, posetive=ccw, negative=cw

        #region CONSTRUCTORS

        public Ellipse2d(Point2d center, double radius1, double radius2, double tilt = 0.0)
        {
            //make sure radius1 is larger than radius2 since we will use it for major axis
            if (Math.Abs(radius1) < Math.Abs(radius2))
            {
                MathUtil.Swap(ref radius1, ref radius2);
                tilt = MathUtil.NormalizeAngle(tilt - MathUtil.Deg90);
            }            

            this.center = center;
            majoraxis = new Vector2d(Math.Cos(tilt) * radius1, Math.Sin(tilt) * radius1);
            sigratio = radius2 / radius1;

            //TODO: normalize???
        }

        public Ellipse2d(Ellipse2d tocopy)
        {
            center = tocopy.center;
            majoraxis = tocopy.majoraxis;
            sigratio = tocopy.sigratio;
        }

        #endregion


        #region CONVERTERS

        public static Ellipse2d FromCircle(Circle2d c)
        {
            return new Ellipse2d(c.Center, c.Radius, c.Radius);
        }

        public override GeneralConic2d ToGeneralConic()
        {
            Transform2d tr = Transform2d.Scale(MajorRadius) * Transform2d.Rotate(Rotation) * Transform2d.Translate(center.X, center.Y);
            GeneralConic2d elcon = new GeneralConic2d(1, 0, 1.0 / (sigratio * sigratio), 0, 0, -1); //x^2+(1/b)^2-1=0 => unit ellipse
            elcon.Transform(tr); //transform conic to position of ellipse
            return elcon; //TODO: optimize this function
        }

        #endregion

        #region ACCESSORS

        public Vector2d MajorAxis
        {
            get
            {
                return majoraxis;
            }
        }

        public Vector2d MinorAxis
        {
            get
            {
                return new Vector2d(-majoraxis.Y*sigratio, majoraxis.X*sigratio);
            }
        }

        public double MajorRadius
        {
            get
            {
                return majoraxis.Length;
            }
        }

        public double MinorRadius
        {
            get
            {
                return Math.Abs(MajorRadius * sigratio); //ratio can be negative on cw ellipses
            }
        }

        public double Ratio
        {
            get
            {
                return Math.Abs(sigratio);
            }
        }

        public Point2d Center
        {
            get { return center; }
            set { center = value; }
        }

        public double Rotation
        {
            get
            {
                return majoraxis.Angle;
            }
        }

        public bool CCW { get { return sigratio > 0.0; } }
        public bool CW { get { return sigratio < 0.0; } }


        #endregion

        public override double Area
        {
            get {
                return Math.PI * MajorRadius * MinorRadius;
            }
        }

        public override double Distance(Point2d p)
        {
            return ClosestPoint(p).Distance(p);
        }

        public override bool IsClosed
        {
            get { return true; }
        }

        public override double Length
        {
            get
            {
                double a = MajorRadius;
                double b = MinorRadius;

                return Numerics.Integrate(
                  delegate(double t)
                  {
                      double si = Math.Sin(t);
                      double co = Math.Cos(t);
                      return Math.Sqrt(a * a * si * si + b * b * co * co);
                  }, 0.0, MathUtil.Deg360, 10000, 1e-9);
            }
        }

             

        private Point2d InternalGetFocus(bool primary) {
                double majrad=MajorRadius;
                double minrad=majrad*sigratio;
                double fdist = Math.Sqrt(majrad * majrad - minrad * minrad);
                return center.Polar(Rotation, fdist * (primary ? 1.0:-1.0));
        }


        

        public Point2d Focus1 { get { return InternalGetFocus(true);}}
        public Point2d Focus2 { get { return InternalGetFocus(false); }}


        public override Point2d PointAt(double t)
        {
            double a=MajorRadius;
            double b=a*sigratio; //negative for cw ellipses
            double r=Rotation;
            double st=Math.Sin(t);
            double ct=Math.Cos(t);
            double sr=Math.Sin(r);
            double cr=Math.Cos(r);
            return new Point2d(
                center.X + a * ct * cr - b * st * sr,
                center.Y + a * ct * sr + b * st * cr);
        }


        public override void Reverse()
        {
            sigratio = -sigratio;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Transform2d ToStandardPosition
        {
            get
            {
                return Transform2d.Translate(-center.X, -center.Y) * Transform2d.Rotate(-majoraxis.Angle) * Transform2d.Scale(1.0 / majoraxis.Length);
            }
        }


        public Point2d Quadrant(int index)
        {
            index = index % 4;
            switch (index)
            {
                case 0: return center + majoraxis;
                case 1: return center + MinorAxis;
                case 2: return center - majoraxis;
                default: return center - MinorAxis; //ie. case 3
            }
        }

        

        public double X {get {return center.X;}}
        public double Y {get {return center.Y;}}


        /// <summary>
        /// Returns all the perpendicular points on the ellipse from a given point 'from'
        /// </summary>
        public Point2d[] Perpendicular(Point2d from)
        {
            
            // Solved by Robert.P. in december 2012
            // Note on solutions:
            // Quartic coefficients gotten from applying lagrange multiplier to minimize (x-i)^2+(y-j)^2
            // with x^2/a^2+y^2/b^2-1=0 as constraint (a=1 because we work in standard position).
            // This gives a system of three equations F_x,F_y,F_lambda, which were solved with
            // resultant theory using 'eliminate' in maxima
            
            //work in standard position, retranslate solutions last
            Transform2d tostd = ToStandardPosition;
            from=from.GetTransformed(tostd);
            
            double b=sigratio,b2=b*b,b4=b2*b2;
            double i=from.X,i2=i*i;
            double j=from.Y,j2=j*j;


            double x4=b4-2*b2+1;
            double x3=2*b2*i-2*i;
            double x2=b2*j2+i2-b4+2*b2-1;
            double x1=2*i-2*b2*i;
            double x0=-i2;

            double[] sols = RealPolynomial.SolveQuartic2(x4, x3, x2, x1, x0,1e-16);
            
            if(sols==null) 
                return null;

            Point2dSet respts = new Point2dSet();

            foreach (double x in sols)
            {
                double y = (1 - x * x) * b2;
                if (y < 0.0)
                    continue;
                y = Math.Sqrt(y);


                for (int l = 0; l < 2; l++)
                {
                    //both posetive and negative y:s can be solutions. Check with each possible
                    //point that its perpendicular to ellipse (subtracting the inverse ellipse slope (=normal slope) with the slope from 'from' point)
                    double err;
                    err = y * (from.X - x) - x * b2 * (from.Y - y);
                    if (Math.Abs(err) < 1e-6)
                        respts.Add(new Point2d(x, y));

                    y = -y; //test negative solution as well
                }
            }

            respts.Transform(tostd.Inversed);
            return respts.ToArray();
        }




        public override Point2d ClosestPoint(Point2d from)
        {
            return from.ClosestPoint(Perpendicular(from));
        }


        public override Vector2d Tangent(Point2d pnt)
        {
            double a=majoraxis.Length;
            double b=MinorAxis.Length;


            pnt = pnt.GetTransformed(ToStandardPosition);

            double slope = -(sigratio * sigratio * pnt.X) / (pnt.Y);
            return Vector2d.FromAngle(Math.Atan(slope) + (pnt.Y < 0.0 ? 0.0:MathUtil.PI)); //TODO: not correct slope, inverted sometimes
        }

        public override Vector2d DirectionAt(double t)
        {
            throw new NotImplementedException();
        }

        public Vector2d Normal(Point2d pt)
        {
            double x=pt.X-center.X;
            double y=pt.Y-center.Y;
            double a=MajorRadius;
            double b=MinorRadius;

            double e2 = x;
            double e1= b*b-a*a-y*y-x*x;
            double e0 = x * a * a - x * b * b;

            double[] roots=RealPolynomial.SolveQuadric(e2, e1, e0);
            if (roots == null) return null;

            Vector2d norm = new Vector2d(x - roots[0], y).Normalized;

            return norm;
        }


        internal static void Ellipse_Transform(ref double a_rh, ref double a_rv, ref double a_offsetrot, ref Point2d endpoint, Transform2d a_mat, out bool a_ReverseWinding)
        {
            double rh, rv, rot;

            double[] m = new double[4];        // matrix representation of transformed ellipse
            double s, c;         // sin and cos helpers (the former offset rotation)
            double A, B, C;       // ellipse implicit equation:
            double ac, A2, C2;  // helpers for angle and halfaxis-extraction.

            rh = a_rh;
            rv = a_rv;
            rot = a_offsetrot;

            s = Math.Sin(rot);
            c = Math.Cos(rot);

            // build ellipse representation matrix (unit circle transformation).
            // the 2x2 matrix multiplication with the upper 2x2 of a_mat is inlined.
            m[0] = a_mat.AX * +rh * c + a_mat.BX * rh * s;
            m[1] = a_mat.AY * +rh * c + a_mat.BY * rh * s;
            m[2] = a_mat.AX * -rv * s + a_mat.BX * rv * c;
            m[3] = a_mat.AY * -rv * s + a_mat.BY * rv * c;
    
            // to implict equation (centered)
            A = (m[0] * m[0]) + (m[2] * m[2]);
            C = (m[1] * m[1]) + (m[3] * m[3]);
            B = (m[0] * m[1] + m[2] * m[3]) * 2.0f;

            // precalculate distance A to C
            ac = A - C;

            // convert implicit equation to angle and halfaxis:
            if (MathUtil.IsZero(B)) //=not tilted
            {
                if (MathUtil.IsZero(ac) || A > C)
                    a_offsetrot = 0;
                else
                    a_offsetrot = MathUtil.Deg90;   



               
                A2 = A;
                C2 = C;
            }
            else
            {
                if (MathUtil.IsZero(ac))
                {
                    A2 = A + B * 0.5f;
                    C2 = A - B * 0.5f;
                    a_offsetrot = MathUtil.PI / 4.0f;
                }
                else
                {
                    // Precalculate radical:
                    double K = 1 + B * B / (ac * ac);

                    // Clamp (precision issues might need this.. not likely, but better safe than sorry)
                    if (K < 0) K = 0; else K = Math.Sqrt(K);

                    A2 = 0.5f * (A + C + K * ac);
                    C2 = 0.5f * (A + C - K * ac);
                    a_offsetrot = 0.5f * Math.Atan2(B, ac);
                }
            }

            // This can get slightly below zero due to rounding issues.
            // it's save to clamp to zero in this case (this yields a zero length halfaxis)
            if (A2 < 0) A2 = 0; else A2 = Math.Sqrt(A2);
            if (C2 < 0) C2 = 0; else C2 = Math.Sqrt(C2);

            // now A2 and C2 are half-axis:
            if (ac <= 0)
            {
                a_rv = A2;
                a_rh = C2;
            }
            else
            {
                a_rv = C2;
                a_rh = A2;
            }

            // The transformation matrix might contain a mirror-component, and the 
            // winding order of the ellise needs to be changed.
            // check the sign of the upper 2x2 submatrix determinant to find out..
            a_ReverseWinding = ((a_mat.AX * a_mat.BY) - (a_mat.AY * a_mat.BX)) < 0 ? true : false;

            // finally, transform ellipse endpoint. This takes care about the
            // translational part which we ignored at the whole math-showdown above.
            endpoint = endpoint.GetTransformed(a_mat);

        }

        public override bool Transform(Transform2d matrix)
        {
            double majax=MajorRadius;
            double minax=MinorRadius;
            double rot=Rotation;
            bool reverse;
            Ellipse_Transform(ref majax, ref minax, ref rot, ref center, matrix, out reverse);

            majoraxis = Vector2d.FromAngleAndLength(rot, majax);
            sigratio = minax / majax;
            
            //TODO: clean up this code
            //TODO: handle reversed ellipse
            
            return true;
        }

        public override double PositionOf(Point2d pnt)
        {
            throw new NotImplementedException();
        }


        

    }
}
