using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{

    public enum ConicType
    {
        Unknown,
        Circle,
        Ellipse,
        Parabola,
        Hyperbola,
        ImaginaryCircle,
        ImaginaryEllipse,
        IntersectingLines,
        ComplexIntersectingLines,
        CoincidentLines,
        ParallelLines,
        ComplexParallelLines
    };

    public class GeneralConic2d:Conic2d
    {
        /// <summary>
        /// Coefficients that describes the conic as ax^2 + bxy + cy^2 + dx + ey + F = 0
        /// </summary>
        private double a, b, c, d, e, f;


        #region CONSTRUCTORS

        public GeneralConic2d(double a, double b, double c, double d,double e, double f)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.e = e;
            this.f = f;
        }

        public GeneralConic2d(GeneralConic2d tocopy)
        {
            a = tocopy.a;
            b = tocopy.b;
            c = tocopy.c;
            d = tocopy.d;
            e = tocopy.e;
            f = tocopy.f;
        }

        #endregion

        #region ACCESSORS

        public double A { get { return a; } set { a = value; } }

        public double B { get { return b; } set { b = value; } }

        public double C { get { return c; } set { c = value; } }

        public double D { get { return d; } set { d = value; } }

        public double E { get { return e; } set { e = value; } }

        public double F { get { return f; } set { f = value; } }

        #endregion

        #region CONVERTERS

        public static GeneralConic2d FromCircle(Circle2d c)
        {

            double i=c.Center.X;
            double j=c.Center.Y;
            double r=c.Radius;

            return new GeneralConic2d(
                1.0,
                0.0,
                1.0,
                -2.0 * i,
                -2.0 * j,
                i * i + j * j - r * r
            );
        }


        public override GeneralConic2d ToGeneralConic() { return new GeneralConic2d(this); } 

       
        
       

        public Conic2d Reduce()
        {
            //double a = A, b = B, c = C, d = D, e = E, f = F;
            
            double na, nb, nc, nd, ne, nf;  //coefficients in virtual coordinate system
            double tilt = 0.0;

            if (B != 0.0)
            { //rotated, adjust factors to work in virtual coordinate system
                tilt = Rotation;

                double cs = Math.Cos(-tilt);
                double si = Math.Sin(-tilt);
                double cs2 = cs * cs;
                double cssi = cs * si;

                //create coefficients of unrotated conic (in virtual coordinate system)
                na = -b * cssi + (a - c) * cs2 + c;
                nb = -(2 * c - 2 * a) * cssi + 2 * b * cs2 - b;
                nc = b * cssi + (c - a) * cs2 + a;
                nd = d * cs - e * si;
                ne = d * si + e * cs;
                nf = f;

                if (!MathUtil.IsZero(nb))
                    return null;    //conic is still rotated => irreducable, this should not happen in normal cases
            }
            else //conic is not rotated
            {
                na = a;
                nb = b;
                nc = c;
                nd = d;
                ne = e;
                nf = f;
            }


            double ztol = 1e-9;

            if (MathUtil.IsZero(na, ztol) && MathUtil.IsZero(nc, ztol))
            {
                //lines => not supported for now. TODO: fix this!
                return null;
            }
            else if (MathUtil.IsZero(na, ztol))
            {
                //horizontal parabola.

                if (MathUtil.IsZero(nd, ztol))
                    return null;    //TODO: return degenerate conic lines

                // compute reduced coefficients
                double c2 = -nc / nd;
                double e2 = -ne / nd;
                double f2 = -nf / nd;

                // vertex of the parabola
                Point2d vertex = new Point2d(
                    -(e2 * e2 - 4 * c2 * f2) / (4 * c2),
                    -e2 * .5 / c2).GetTransformed(Transform2d.Rotate(tilt));

                // create and return result
                double focdist = 1.0 / (4.0 * c2);
                if (focdist < 0.0)
                { //try to keep posetive focal distance by altering rotation
                    focdist = -focdist;
                    tilt -= MathUtil.Deg180;
                }
                return new Parabola2d(vertex, MathUtil.NormalizeAngle(tilt - MathUtil.Deg90), focdist);

            }
            else if (MathUtil.IsZero(nc, ztol))
            {
                // vertical parabola
                if (MathUtil.IsZero(ne, ztol))
                    return null;    //TODO: return degenerate conic lines


                // compute reduced coefficients:
                double a2 = -na / ne;
                double d2 = -nd / ne;
                double f2 = -nf / ne;

                // vertex of parabola
                Point2d vertex = new Point2d( //vertex in unrotated sysem of conic...
                    -d2 * 0.5 / a2,
                    -(d2 * d2 - 4 * a2 * f2) / (4 * a2)).GetTransformed(Transform2d.Rotate(tilt)); //...rotated to conic system

                // create and return result
                double focdist = 1.0 / (4.0 * a2);
                if (focdist < 0.0)
                { //try to keep posetive focal distance
                    focdist = -focdist;
                    tilt -= MathUtil.Deg180;
                }

                return new Parabola2d(vertex, MathUtil.NormalizeAngle(tilt), focdist);
            }
            else
            {
                //ellipse or hyperbola

                // get length of major and minor axis
                double num = (nc * nd * nd + na * ne * ne - 4 * na * nc * nf) / (4 * na * nc);
                double sqmajax = num / na;   //axes, squared length with sign
                double sqminax = num / nc;

                if (MathUtil.IsZero(sqmajax, ztol) || MathUtil.IsZero(sqminax, ztol))
                    return null;    //dont allow zero axes

                if (sqmajax <= 0.0 && sqminax <= 0.0)
                    return null;    //both axes cannot be negative

                if (sqmajax > 0.0 && sqminax > 0.0)
                    //ellipse => constructor fixes bt>at by rotating ellipse
                    return new Ellipse2d(Center, Math.Sqrt(sqmajax), Math.Sqrt(sqminax), tilt);
                else
                {
                    if (sqmajax > 0.0) //vertical line of symetry => as we define hyperbola
                        return new Hyperbola2d(Center, Math.Sqrt(Math.Abs(sqmajax)),Math.Sqrt(Math.Abs(sqminax)), tilt);
                    else //horizontal line of symetry => we have to rotate to fit our hyperbola definition
                        return new Hyperbola2d(Center, Math.Sqrt(Math.Abs(sqminax)), Math.Sqrt(Math.Abs(sqmajax)), tilt+MathUtil.Deg90);
                }
            }
        }

        public Matrix ToMatrix()
        {
            double b05=b*0.5;
            double d05=d*0.5;
            double e05=e*0.5;
            return new Matrix(3, 3, a, b05, d05, b05, c, e05, d05, e05, f);
        }

        public static GeneralConic2d FromMatrix(Matrix m)
        {
            if (m.NumRows != 3 || m.NumColumns != 3)
                throw new Exception("Conic2.FromMatrix => wrong size of matrix, must be 3x3");

            return new GeneralConic2d(m[0, 0], m[0, 1] + m[1, 0], m[1, 1], m[2, 0] + m[0, 2], m[1, 2] + m[2, 1], m[2, 2]);
        }

        #endregion


        public override double Area
        {
            get { throw new NotImplementedException(); }
        }

        public override double Distance(Point2d p)
        {
            throw new NotImplementedException();
        }

        public override double Length
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

       

        public override Point2d PointAt(double t)
        {
            throw new NotImplementedException();
        }

        public override void Reverse()
        {
            throw new NotImplementedException();
        }

        public double Determinant
        {
            get
            {
                double A = a, B = b / 2, C = c, D = d / 2, E = e / 2, F = f;
                return A * (C * F - E * E) - B * (B * F - D * E) + D * (B * E - C * D);
            }
        }

        public Transform2d ToStandardPosition
        {
            get
            {
                //standard position means that x^2 term (a)=1 and xy term(b)=0.0 (no rotation)
                //TODO: check this is in line with IGES specification of standard position
                //TODO: test for parabola and hyperbola, only tested for ellipse! (especially downscaling with 'scale')

                double r = Rotation;
                Point2d cen = Center;
                
                //compute scaling factor to scale so that major radius becomes 1.0
                double scale = 1.0;
               /* double si = Math.Sin(r);
                double cs = Math.Cos(r);
                double nu = a * Center.X * Center.X + b * Center.Y * Center.Y + c * Center.Y * Center.Y - f;
                double denom = a * cs * cs + b * cs * si + c * si * si; // nu/denom is major radius
                if (!MathUtil.IsZero(nu, 1e-9)) //not degenerate
                    scale = Math.Sqrt(Math.Abs(denom / nu));*/


                Transform2d res = Transform2d.Translate(-cen.X, -cen.Y) * Transform2d.Rotate(-r);// *Transform2d.Scale(scale);

              


                return res;
            }
        }

        public bool IsDegenerate { get { return MathUtil.IsZero(Determinant, 1e-16); }}

        public ConicType Type
        {
            get
            {
                double A = a, B = b / 2, C = c, D = d / 2, E = e / 2, F = f;


                /* determinant, subdeterminants and trace values */
                double det = A * (C * F - E * E) - B * (B * F - D * E) + D * (B * E - C * D); // determinant
                double J = A * C - B * B;  // upper 2x2 determinant
                double K = (C * F - E * E) + (A * F - D * D); // sum of two other 2x2 determinants
                double I = A + C; // trace of upper 2x2

                double zerotol = 1e-10; // 1e-6; 

                if (!MathUtil.IsZero(det,zerotol))
                {
                    if (J > 0)
                    {
                        if (det * I < 0)
                        {
                            if (A == C && B == 0) return ConicType.Circle;
                            else return ConicType.Ellipse;
                        }
                        else
                        {
                            if (A == C && B == 0) return ConicType.ImaginaryCircle;
                            else return ConicType.ImaginaryEllipse;
                        }
                    }
                    else if (J < 0) return ConicType.Hyperbola;
                    else /* J == 0 */ return ConicType.Parabola;
                }
                else
                {    // limiting cases
                    if(MathUtil.IsZero(J,zerotol))
                    {
                        if (MathUtil.IsZero(A,zerotol) && MathUtil.IsZero(B,zerotol) && MathUtil.IsZero(C,zerotol))
                        { // line at infinity is component
                            if (!MathUtil.IsZero(D,zerotol) || !MathUtil.IsZero(E,zerotol)) 
                                return ConicType.IntersectingLines;
                            else if (!MathUtil.IsZero(F,zerotol)) 
                                return ConicType.CoincidentLines; // 2x w=0
                            else 
                                return ConicType.Unknown; // all coefficients are 0
                        }
                        else if(MathUtil.IsZero(K,zerotol))
                            return ConicType.CoincidentLines;
                        else if (K < 0) return ConicType.ParallelLines;
                        else //K>0 
                            return ConicType.ComplexParallelLines;

                    }
                    else if (J < 0.0) 
                        return ConicType.IntersectingLines;
                    else //j>0 
                        return ConicType.ComplexIntersectingLines;
                    
                }
            }
        }


      

        public Ellipse2d ToEllipse()
        {

            double cx, cy, cs, si, tilt, majrad, minrad, majrad_sq, minrad_sq, de, nu;

            de = 4 * a * c - b * b;

            if (MathUtil.IsZero(de, 1e-30)) return null; //degenerate
            tilt = Rotation;
            cx = (b * e - 2 * c * d) / de;
            cy = (b * d - 2 * a * e) / de;
            nu = a * cx * cx + b * cx * cy + c * cy * cy - f;
            
            cs = Math.Cos(tilt);
            si = Math.Sin(tilt);

            if ((de = a * cs * cs + b * cs * si + c * si * si) == 0) return null;
            if ((majrad_sq = nu / de) < 0.0) return null;
            majrad = Math.Sqrt(majrad_sq);
            if ((de = c * cs * cs - b * cs * si + a * si * si) == 0.0) return null;
            if ((minrad_sq = nu / de) < 0.0) return null;
            minrad = Math.Sqrt(minrad_sq);

            return new Ellipse2d(new Point2d(cx, cy), majrad, minrad, tilt);
        }

        /// <summary>
        /// Gets the line geometries, one or two lines. The type of the conic has to be
        /// Intersecting lines (2 results), Paralell lines (2 results) or Coincident lienes (1 result),
        /// otherwise the result is null.
        /// </summary>
        /// <returns></returns>
        public Line2d[] ToLines()
        {

            // Inspired by line extraction conmat.c from book Graphics Gems V

            double xx, yy;
            ConicType type = Type;
            Line2d tmplin;
            Transform2d tr;
            GeneralConic2d cpy;


            List<Line2d> res = null;



            double de = B * B*0.25 - A * C;

            if (MathUtil.IsZero(A) && MathUtil.IsZero(B) && MathUtil.IsZero(C))
            { //single line
                // compute endpoints of the line, avoiding division by zero
                res = new List<Line2d>();
                if (Math.Abs(d) > Math.Abs(e))
                    res.Add(new Line2d(-f / (d), 0.0, -(e + f) / (d), 1.0));
                else
                    res.Add(new Line2d(0.0, -f / (e), 1.0, -(d + f) / (e)));
            }
            else
            { // two lines

                cpy = new GeneralConic2d(this);
                double a = cpy.a, b = cpy.b * 0.5, c = cpy.c, d = cpy.d * 0.5, e = cpy.e * 0.5, f = cpy.f; //get matrix coefficient

                // use the expression for phi that takes atan of the smallest argument
                double phi = (Math.Abs(b + b) < Math.Abs(a - c) ?
                    Math.Atan((b + b) / (a - c)) :
                    MathUtil.Deg360 - Math.Atan((a - c) / (b + b))) / 2.0;
                
                //phi = cpy.Rotation;

                if (MathUtil.IsZero(de))
                { //parallel lines
                    tr = Transform2d.Rotate(-phi);
                    cpy.Transform(tr);
                    a = cpy.A; b = cpy.B * 0.5; c = cpy.c; d = cpy.d * 0.5; e = cpy.e * 0.5; f = cpy.f; //get matrix coefficient

                    if (Math.Abs(c) < Math.Abs(a))   // vertical 
                    {

                        double[] xs = RealPolynomial.SolveQuadric(a, d, f);
                        if (xs != null)
                        {
                            res = new List<Line2d>();
                            foreach (double x in xs)
                            {
                                tmplin = new Line2d(x, -1, x, 1);
                                tmplin.Transform(tr.Inversed);  //back to original spacxe
                                res.Add(tmplin);
                            }
                        }
                    }
                    else //horizontal
                    {
                        double[] ys = RealPolynomial.SolveQuadric(c, e, f, 0.0);
                        if (ys != null)
                        {
                            res = new List<Line2d>();
                            foreach (double y in ys)
                            {
                                tmplin = new Line2d(-1, y, 1, y);
                                tmplin.Transform(tr.Inversed);
                                res.Add(tmplin);
                            }
                        }
                    }

                } //end parallel lines case
                else
                { //crossing lines
                    Point2d center = Center;
                    double rot = this.Rotation;
                    tr = Transform2d.Translate(-center.X, -center.Y) * Transform2d.Rotate(-rot);
                    cpy.Transform(tr);
                    a = cpy.A; b = cpy.B * 0.5; c = cpy.c; d = cpy.c * 0.5; e = cpy.e * 0.5; f = cpy.f;

                    res = new List<Line2d>();

                    xx = Math.Sqrt(Math.Abs(1.0 / a));
                    yy = Math.Sqrt(Math.Abs(1.0 / c));
                    tr = tr.Inversed;
                    tmplin = new Line2d(-xx, -yy, xx, yy);
                    tmplin.Transform(tr);
                    res.Add(tmplin);
                    tmplin = new Line2d(new Line2d(-xx, yy, xx, -yy));
                    tmplin.Transform(tr);
                    res.Add(tmplin);
                } //end crossing lines case
            }   //end two lines


           

            if (res == null || res.Count == 0)
                return null;

            return res.ToArray();
        }


       /* public List<Line2d> ToLinesNew()
        {
            List<Line2d> res = null;

            Point2d center = Center;
            double angle = Rotation;
            Transform2d conmat = Transform2d.Rotate(angle) * Transform2d.Translate(center.X, center.Y);

            GeneralConic2d con=new GeneralConic2d(this); //modify a copy
            con.Transform(conmat.Inversed);

            if (MathUtil.IsZero(con.a) && MathUtil.IsZero(con.c))
            {
                //no quadric terms => single line
                Line2d.FromEquation(con.D, con.E, con.f); //TODO: test
            }
            else if (MathUtil.IsZero(con.a) && !MathUtil.IsZero(con.c))
            {
                // horizontal parallel lines
                double y = -con.f / con.c;
                if (y < 0.0) return null; //imaginary parallel lines

                res = new List<Line2d>();
                y = Math.Sqrt(y);
                res.Add(new Line2d(0.0,y,1.0,y));
                res.Add(new Line2d(0.0, -y, 1.0, -y));
            }
            else if (!MathUtil.IsZero(con.a) && MathUtil.IsZero(con.c))
            {
                // vertical parallel lines
                double x = -con.f / con.a;
                if (x < 0.0) return null; //imaginary parallel lines

                res = new List<Line2d>();
                x = Math.Sqrt(x);
                res.Add(new Line2d(x, 0.0, x, 1.0));
                res.Add(new Line2d(-x, 0.0, -x, 1.0));
            }
            {
                double det = Determinant;   //todo: correct?? what is this determinant, check it? compare to conmat.c



            }


            return null;
        }*/

        

        public double Rotation
        {
            get
            {
                //see http://en.wikipedia.org/wiki/Rotation_of_axes for an explanation
                double asc = a - c;
                if (MathUtil.IsZero(asc, 1e-9))
                    return MathUtil.Deg45;

                double t = Math.Atan(b / asc) * 0.5;                

                // keep result between 0-360 degrees
                return MathUtil.NormalizeAngle(t);

            }
        }

        public Point2d Center
        {
            get
            {
                if (!IsDegenerate)
                {
                    // see http://www.ping.be/math/center.htm
                    double det = 4 * a * c - b * b;
                    return new Point2d((b * e - 2 * c * d) / det, (b * d - 2 * a * e) / det);
                }
                else // a degenerate conic has no dual; in this case, return the centre:
                    if (MathUtil.IsZero(a, 1e-16) && MathUtil.IsZero(1e-16) && MathUtil.IsZero(d)) // two horizontal lines
                        return new Point2d(1, 0);
                    else if (MathUtil.Equals(a * c * 4, b * b) && MathUtil.Equals(a * e * 2, b * d))
                    {
                        double w = a * e * 2 - b * d;
                        return new Point2d((b * f * 2 - e * d)/w, (d * d - a * f * 4)/w);
                    }
                    else
                    {
                        double w = a * c * 4 - b * b;
                        return new Point2d((b * e - c * d * 2)/w,(b * d - a * e * 2)/w);
                    }
            }
        }

       
        public GeneralConic2d DualConic {
            get
            {
                //TODO: optimize this a bit, get rid of pre-divided cofs.
                double b05 = b * 0.5, d05 = d * 0.5, e05 = e * 0.5;
                return new GeneralConic2d(
                    e05 * e05 - c * f, 
                    2 * (b05 * f - d05 * e05), 
                    d05 * d05 - a * f, 
                    2 * (c * d05 - b05 * e05), 
                    2 * (a * e05 - b05 * d05), 
                    b05 * b05 - a * c
                );
            }
        }

        public override string ToString()
        {
            List<string> parts = new List<string>();
            StringBuilder sb = new StringBuilder();

            if (a != 0.0) sb.Append(a.ToString() + "x²");
            if (b != 0.0) sb.Append((b>0.0 ? " + ":" - ") + Math.Abs(b).ToString() + "xy");
            if (c != 0.0) sb.Append((c > 0.0 ? " + " : " - ") + Math.Abs(c).ToString() + "y²");
            if (d != 0.0) sb.Append((d > 0.0 ? " + " : " - ") + Math.Abs(d).ToString() + "x");
            if (e != 0.0) sb.Append((e > 0.0 ? " + " : " - ") + Math.Abs(e).ToString() + "y");
            if (f != 0.0) sb.Append((f > 0.0 ? " + " : " - ") + Math.Abs(f).ToString());

            return sb.ToString();
        }

        public override bool Transform(Transform2d tr)
        {
            //Inspired by conmat.c from graphics gems V, but a lot simplified

            // Compute M' = Inv(TMat).M.Transpose(Inv(TMat))
            Matrix inv = tr.Inversed.ToMatrix();
            //Matrix conic = inv.Transposed * ToMatrix() * inv;
            Matrix conic = inv.Transposed * ToMatrix() * inv;
            
            a = conic[0, 0];	       // return to conic form
            b = conic[0, 1] + conic[1, 0];
            c = conic[1, 1];
            d = conic[0, 2] + conic[2, 0];
            e = conic[1, 2] + conic[2, 1];
            f = conic[2, 2];

            return true;
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


        public override Vector2d DirectionAt(double t)
        {
            throw new NotImplementedException();
        }

        

    }
}
