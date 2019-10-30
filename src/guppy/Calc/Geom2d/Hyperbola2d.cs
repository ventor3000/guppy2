using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{
    /// <summary>
    /// Implements a hyperbola on the form x^2/a^2-y^2/b^2-1=0
    /// </summary>
    public class Hyperbola2d:Conic2d
    {
        Point2d center;
        Vector2d majoraxis;  //a = magnitude, angle=tilt
        double ratio;       //the to a of factor b => b=a*ratio

        public Hyperbola2d(Point2d center, double a, double b, double tilt = 0.0)
        {
            if (a<=0.0 || b <= 0.0)
                throw new Exception("Both axes of hyperbola has to be posetive"); //TODO: why?

            majoraxis = Vector2d.FromAngleAndLength(tilt, a);
            this.center = center;
            ratio = b / a;
        }

        public Hyperbola2d(Hyperbola2d tocopy)
        {
            center = tocopy.center;
            majoraxis = tocopy.majoraxis;
            ratio = tocopy.Ratio;
        }

        #region ACCESSORS

        public double A
        {
            get
            {
                return majoraxis.Length;
            }
        }

        public double B
        {
            get
            {
                return majoraxis.Length * ratio;
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
            set
            {
                majoraxis = Vector2d.FromAngleAndLength(value, majoraxis.Length);
            }
        }

        public double Ratio
        {
            get
            {
                return ratio;
            }
        }

        #endregion


        #region CONVERTERS


        public override GeneralConic2d ToGeneralConic()
        {
            //TODO: test this function

            Transform2d tr = Transform2d.Scale(majoraxis.Length) * Transform2d.Rotate(Rotation) * Transform2d.Translate(center.X, center.Y);
            GeneralConic2d hypcon = new GeneralConic2d(1, 0, -1.0 / (ratio * ratio), 0, 0, -1); //x^2-(y/b)^2-1=0 => unit 
            hypcon.Transform(tr);

            return hypcon; //TODO: optimize this function
        }

        #endregion

        /// <summary>
        /// Computes a transform that will transform this hyperbola to standrad postion, that is:
        /// center=0.0  tilt=0.0 a=1.0  b=ratio
        /// </summary>
        public Transform2d ToStandardPosition
        {
            get
            {
                return Transform2d.Translate(-center.X, -center.Y) * Transform2d.Rotate(-Rotation) * Transform2d.Scale(1.0 / A);
            }
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
            get { throw new NotImplementedException(); }
        }

       
        public override Point2d PointAt(double t)
        {
            double a = A;
            double b = B;
            double r = Rotation;

            
            if (r == 0.0)   //simpler evaluation for unrotated hyperbola
            {
                return new Point2d(
                    a * MathUtil.Sec(t) + center.X,
                    b * Math.Tan(t) + center.Y
                );
            }
            else
            {
                double st = Math.Tan(t);
                double ct = MathUtil.Sec(t);
                double sr = Math.Sin(r);
                double cr = Math.Cos(r);
                return new Point2d(
                    center.X + a * ct * cr - b * st * sr,
                    center.Y + a * ct * sr + b * st * cr);
            }
        }

        public override void Reverse()
        {
            throw new NotImplementedException();
        }

        public override Vector2d Tangent(Point2d where)
        {
            throw new NotImplementedException();
        }

        public override Point2d ClosestPoint(Point2d from)
        {
            throw new NotImplementedException();
        }

        public override double PositionOf(Point2d pnt)
        {
            throw new NotImplementedException();
        }

        public override double Distance(Point2d p)
        {
            throw new NotImplementedException();
        }

        internal static void Hyperbola_Transform(ref double a_rh, ref double a_rv, ref double a_offsetrot, ref Point2d endpoint, Transform2d a_mat, out bool a_ReverseWinding)
        {

            //TODO: test this function throughly, it is a modified Ellipse_Transform

            
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
                a_offsetrot = 0;
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


            a_rv = C2;
            a_rh = A2;

           

            // The transformation matrix might contain a mirror-component, and the 
            // winding order of the ellise needs to be changed.
            // check the sign of the upper 2x2 submatrix determinant to find out..
            a_ReverseWinding = ((a_mat.AX * a_mat.BY) - (a_mat.AY * a_mat.BX)) < 0 ? true : false;

            // finally, transform ellipse endpoint. This takes care about the
            // translational part which we ignored at the whole math-showdown above.
            endpoint = endpoint.GetTransformed(a_mat);

        }

        public override bool Transform(Transform2d t)
        {
            //transform hyperbola centered at origin, because it's more stable general conic
            Hyperbola2d hyp = new Hyperbola2d(this);
            hyp.center = Point2d.Origo;
            var gencon=hyp.ToGeneralConic();

            if (!gencon.Transform(new Transform2d(t.AX, t.AY, t.BX, t.BY, 0.0, 0.0)))
                return false;

            hyp = gencon.Reduce() as Hyperbola2d;
            if (hyp == null)
                return false;

            //now transform centerpoint separately,
            //and write bac to this
            center = center.GetTransformed(t);
            ratio = hyp.ratio;
            majoraxis = hyp.majoraxis;

            return true;
           

           /* double majax = majoraxis.Length;
            double minax = -majax * ratio;
            double rot = Rotation;
            bool reverse;
            Hyperbola_Transform(ref majax, ref minax, ref rot, ref center, t, out reverse);

            majoraxis = Vector2d.FromAngleAndLength(rot, majax);
            ratio = minax / majax;

            return true;*/
        }

        public override Vector2d DirectionAt(double t)
        {
            throw new NotImplementedException();
        }

        
        
    }
}
