using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc
{
    public class RealPolynomial
    {
        double[] cof; // [x^0 , x^1 , x^2 , x^3 etc.]

        public RealPolynomial(params double[] c)
        {
            int n = c.Length;
            cof = new double[n];
            foreach (double d in c)
                cof[--n] = d; //reverse order we want to store quadric term at place 2 etc.
            Clean();
        }

        private RealPolynomial()
        {

        }
       
        private static RealPolynomial FromDegree(int degree) {
            RealPolynomial rp=new RealPolynomial();
            rp.cof = new double[degree + 1];
            return rp;
        }


        public static RealPolynomial FromRoots(params double[] roots) {
            if(roots.Length<=0)
                return new RealPolynomial(0);

            RealPolynomial res=new RealPolynomial(1,-roots[0]);

            for(int l=1;l<roots.Length;l++) 
                res=res*new RealPolynomial(1,-roots[l]);

            return res;
        }

        /// <summary>
        /// Removes zero valued coeficients first in polynomial.
        /// Returns true if anything was removed.
        /// </summary>
        public bool Clean(double zerotol=1e-20)
        {
            int n = cof.Length;
            bool res = false;
            //dont remove the last (constant) coeficient ever
            while (n > 1 && MathUtil.IsZero(cof[n - 1], zerotol)) { res = true; n--; }
            if (res)
                Array.Resize<double>(ref cof, n);
            return res;
        }

        public int Degree
        {
            get
            {
                return cof.Length - 1;
            }
        }

        public void Differentiate()
        {
            cof = DerivativeCoefficients(cof);
        }

        public double Eval(double x)
        {
            return EvalCoefficients(cof, x);
        }

        public RealPolynomial Derivative
        {
            get
            {
                RealPolynomial res = new RealPolynomial();
                res.cof = DerivativeCoefficients(cof);
                return res;
            }
        }

        public double this[int idx]
        {
            get
            {
                if (idx < 0 || idx >= cof.Length)
                    return 0.0;
                return cof[idx];
            }
            set
            {
                if (idx < 0)
                    return;
                if (idx >= cof.Length)
                    Array.Resize(ref cof, idx + 1);
                cof[idx] = value;
            }
        }

        public static RealPolynomial operator +(RealPolynomial a, RealPolynomial b)
        {
            int n = Math.Max(a.cof.Length, b.cof.Length);
            double[] rc = new double[n];
            for (int i = 0; i < n; i++)
                rc[i] = a[n - i - 1] + b[n - i - 1];
            return new RealPolynomial(rc);
        }

        public static RealPolynomial operator -(RealPolynomial a, RealPolynomial b)
        {
            int n = Math.Max(a.cof.Length, b.cof.Length);
            double[] rc = new double[n];
            for (int i = 0; i < n; i++)
                rc[i] = a[n - i - 1] - b[n - i - 1];
            return new RealPolynomial(rc);
        }

        public static RealPolynomial operator *(RealPolynomial a, RealPolynomial b)
        {
            int alen = a.cof.Length;
            int blen = b.cof.Length;


            RealPolynomial res = FromDegree(alen + blen - 1);

            for (int bi = 0; bi < blen; bi++)
                for (int ai = 0; ai < alen; ai++)
                    res[ai + bi] += a.cof[ai] * b.cof[bi];

            res.Clean();
            return res;
        }

        public static RealPolynomial operator /(RealPolynomial a, RealPolynomial b)
        {
            RealPolynomial res, mod;
            a.DivMod(b, out res, out mod);
            return res;
        }

        public static RealPolynomial operator %(RealPolynomial a, RealPolynomial b)
        {
            RealPolynomial res, mod;
            a.DivMod(b, out res, out mod);
            return mod;
        }

        public static RealPolynomial operator -(RealPolynomial a)
        {
            var res = a.Copy();
            for (int l = 0; l < res.cof.Length; l++)
                res.cof[l] = -res.cof[l];
            return res;
        }

        private RealPolynomial MultPower(int power, double m)
        {
            RealPolynomial newpoly = FromDegree(power + Degree + 1);
            for (int idx = 0; idx < cof.Length; idx++)
                newpoly[idx + power] = cof[idx] * m;

            newpoly.Clean();

            return newpoly;
        }

        public RealPolynomial Copy()
        {
            RealPolynomial res = FromDegree(Degree);
            for (int l = 0; l < cof.Length; l++)
                res[l] = this[l];
            return res;
        }

        public void DivMod(RealPolynomial b, out RealPolynomial result, out RealPolynomial mod)
        {
            RealPolynomial numerator = Copy();
            RealPolynomial denominator = b;
            result = new RealPolynomial(0.0);
            // numerator / denominator = result + remainder / denominator

            int curdeg;
            double curfac;
            do
            {
                curdeg = numerator.Degree - denominator.Degree;
                if (curdeg < 0)
                    break;

                curfac = numerator[numerator.Degree] / denominator[denominator.Degree];
                numerator -= (denominator.MultPower(curdeg, curfac));
                result[curdeg] += curfac;

                if (numerator.Degree == 0)
                    break;

            } while (numerator.Degree >= denominator.Degree && numerator.Degree >= 0);

            mod = numerator;
        }

        public RealPolynomial Pow(int power)
        {
            if (power < 0)
                throw new ArgumentException("RealPolynomial.Pow can only use posetive powers");
            else if (power == 1)
                return new RealPolynomial(1.0);

            RealPolynomial result = Copy();
            for (int l = 1; l < power; l++)
                result *= this;

            return result;
        }

        /// <summary>
        /// Returns an array with the real solutions of euqtion ax^2+bx+c=0
        /// This array can be 0,1 or 2 elements long.
        /// </summary>
        public static double[] SolveQuadric(double a,double b,double c,double zerotolerance=0.0)
        {
            
            double[] s=null;

            double p, q, d;


            if (MathUtil.IsZero(a))
                return SolveLinear(b, c);

            p = b / (2.0 * a);
            q = c / a;

            d = p * p - q;

            if (MathUtil.IsZero(d,zerotolerance))
            {
                s = new double[1];
                s[0] = -p;
            }
            else if (d >= -zerotolerance) //allow for slightly negative values
            {
                if (d < 0.0) d = 0.0; //set d to 0 if within tolerance

                double sr = Math.Sqrt(d);
                s = new double[2];
                s[0] = -sr - p;
                s[1] = sr - p;
            }
           

            return s;
        }

        /// <summary>
        /// Returns an array with the solution of the equation ax+b
        /// This array can 1 elements long or null is returned.
        /// </summary>
        public static double[] SolveLinear(double a, double b)
        {
            if (MathUtil.IsZero(a))
                return null;
            return new double[1] { -b / a };
        }
       
        public static double[] SolveCubic2(double c3, double c2, double c1, double c0,double tol=1e-9)
        {
            // see Graphics Gems I book

            int i, num;
            double sub;
            double A, B, C;
            double sq_A, p, q;
            double cb_p, D;
            double[] s = new double[3];   //solutions

            /* normal form: x^3 + Ax^2 + Bx + C = 0 */

            A = c2 / c3;
            B = c1 / c3;
            C = c0 / c3;

            if (double.IsInfinity(A) || double.IsInfinity(B) || double.IsInfinity(C)) //ie. c3 is about zero
                return SolveQuadric(c2, c1, c0, tol);  

            /*  substitute x = y - A/3 to eliminate quadric term:
            x^3 +px + q = 0 */

            sq_A = A * A;
            p = 1.0 / 3 * (-1.0 / 3 * sq_A + B);
            q = 1.0 / 2 * (2.0 / 27 * A * sq_A - 1.0 / 3 * A * B + C);

            /* use Cardano's formula */

            cb_p = p * p * p;
            D = q * q + cb_p;

            if (MathUtil.IsZero(D, tol))
            {
                if (MathUtil.IsZero(q, tol)) /* one triple solution */
                {
                    s[0] = 0;
                    num = 1;
                }
                else /* one single and one double solution */
                {
                    double u = MathUtil.CubeRoot(-q);
                    s[0] = 2 * u;
                    s[1] = -u;
                    num = 2;
                }
            }
            else if (D < 0) /* Casus irreducibilis: three real solutions */
            {
                double phi = 1.0 / 3 * Math.Acos(-q / Math.Sqrt(-cb_p));
                double t = 2 * Math.Sqrt(-p);

                s[0] = t * Math.Cos(phi);
                s[1] = -t * Math.Cos(phi + MathUtil.PI / 3);
                s[2] = -t * Math.Cos(phi - MathUtil.PI / 3);
                num = 3;
            }
            else /* one real solution */
            {
                double sqrt_D = Math.Sqrt(D);
                double u = MathUtil.CubeRoot(sqrt_D - q);
                double v = -MathUtil.CubeRoot(sqrt_D + q);

                s[0] = u + v;
                num = 1;
            }

            /* resubstitute */

            sub = 1.0 / 3 * A;

            for (i = 0; i < num; ++i)
                s[i] -= sub;

            if (num == 0) return null; //can never happen for 3:d degree equations
            if (num != 3) Array.Resize(ref s, num);
            return s;
        }

        

        /// <summary>
        /// Returns an array with the real solutions of eqution ax^4+bx^3+cx^2+dx+e=0
        /// This array can be 1 to 4 elements long. If no solutions found, null is returned.
        /// </summary>
        public static double[] SolveQuartic2(double c4, double c3, double c2, double c1, double c0,double tol=1e-9)
        {
            double z, u, v, sub;
            double A, B, C, D;
            double sq_A, p, q, r;
            int i, num=0;
            double[] s = new double[4]; //solutions
            double[] subsol;

            /* normal form: x^4 + Ax^3 + Bx^2 + Cx + D = 0 */

            

            A = c3 / c4;
            B = c2 / c4;
            C = c1 / c4;
            D = c0 / c4;

            if (double.IsInfinity(A) || double.IsInfinity(B) || double.IsInfinity(C) || double.IsInfinity(D)) //ie. c4 is about 0
                return SolveCubic2(c3, c2, c1, c0, tol);

            /*  substitute x = y - A/4 to eliminate cubic term:
            x^4 + px^2 + qx + r = 0 */

            sq_A = A * A;
            p = -3.0 / 8 * sq_A + B;
            q = 1.0 / 8 * sq_A * A - 1.0 / 2 * A * B + C;
            r = -3.0 / 256 * sq_A * sq_A + 1.0 / 16 * sq_A * B - 1.0 / 4 * A * C + D;

            if (MathUtil.IsZero(r, tol))
            {
                /* no absolute term: y(y^3 + py + q) = 0 */
                foreach (double ss in SolveCubic2(1, 0, p, 1,tol))
                    s[num++] = ss;
                s[num++] = 0;
            }
            else
            {
                /* solve the resolvent cubic ... */
                /* ... and take the one real solution ... */
                z = SolveCubic2(1, -1.0 / 2 * p, -r, 1.0 / 2 * r * p - 1.0 / 8 * q * q,tol)[0];

                /* ... to build two quadric equations */

                u = z * z - r;
                v = 2 * z - p;

                if (MathUtil.IsZero(u, tol))
                    u = 0;
                else if (u > 0)
                    u = Math.Sqrt(u);
                else
                    return null;

                if (MathUtil.IsZero(v, tol))
                    v = 0;
                else if (v > 0)
                    v = Math.Sqrt(v);
                else
                    return null;

                subsol=SolveQuadric(1, q < 0 ? -v : v, z - u,tol);
                if(subsol!=null)
                    foreach (double ss in subsol)
                        s[num++] = ss;

                subsol=SolveQuadric(1, q < 0 ? v : -v, z + u,tol);
                if(subsol!=null)
                    foreach (double ss in subsol)
                        s[num++] = ss;
            }

            /* resubstitute */
            sub = 1.0 / 4 * A;

            for (i = 0; i < num; ++i)
                s[i] -= sub;

            if (num == 0) 
                return null;
            else if (num != 4)
                Array.Resize(ref s, num);
            return s;
        }

        /// <summary>
        /// Divides all coefficients with the largest coefficients, so that
        /// all coefficients becomes between -1 and 1
        /// </summary>
        public void Normalize(double maxfactor=1.0)
        {

            //TODO: normalize should make larges coeff. 1.0 and nothing else

            double div = 0.0;
            foreach (double d in cof)
                div = Math.Max(div, Math.Abs(d));
            
            if(maxfactor!=1.0)
                div /= maxfactor;

            if (!MathUtil.IsZero(div))
            {
                int i = 0;
                foreach (var d in cof)
                    cof[i++] = d / div;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int l = 0; l < cof.Length; l++)
            {
                int pos = cof.Length - 1 - l;
                double d = cof[pos];

                if (l == 0)
                {
                    if (d < 0.0)
                        sb.Append('-');
                }
                else
                {
                    if (d < 0.0)
                        sb.Append(" - ");
                    else
                        sb.Append(" + ");
                }

                if (pos == 0 || d != 1.0) //dont add 1 coefficients except on constant coef.
                    sb.Append(StringUtil.FormatReal(Math.Abs(d)));

                if (pos >= 2)
                    sb.Append("x^" + StringUtil.FormatInt(pos));
                else if (pos == 1)
                    sb.Append("x");
            }

            return sb.ToString();
        }



        private void PushRoot(ref List<double> roots,double rootx)
        {
            //adds a root to an array which is possibly created,
            //including some sanity checks
            if (roots == null)
            {
                roots = new List<double>();
                roots.Add(rootx);
            }
            else
            {
                if (roots[roots.Count-1] < rootx)    //avoid exactly equal roots
                    roots.Add(rootx);
            }
        }

        private double[] NumericFindRoots(double tol,double zerotol)
        {
            List<double> res=null;

            if (Degree == 0)
                return null;
            else if (Degree == 1)
                return SolveLinear(cof[1], cof[0]);
            else if(Degree==2)
                return SolveQuadric(cof[2], cof[1], cof[0], zerotol);
            else {
                double xmin,xmax,rootx;
                GetRootBounds(out xmin, out xmax);
                double[] minmax=Derivative.NumericFindRoots(1e-12,zerotol); //roots of derivative is extremas of function, be soft on acceptable tolerance here...
                if (minmax != null)
                {

                    foreach (double x in minmax)
                    {
                        if (Numerics.Brent(xmin, x, (xp) => Eval(xp), out rootx,tol,zerotol))
                            PushRoot(ref res, rootx);
                        xmin = x;
                    }
                }
                if (Numerics.Brent(xmin, xmax, (xp) => Eval(xp), out rootx,tol,zerotol))
                    PushRoot(ref res, rootx);
            }

            if (res == null)
                return null;
            return res.ToArray();
        }



        public double[] FindRoots(bool forcenumeric = false, double goal_error = 1e-12,double zerotol=MathUtil.Epsilon)
        {
            int d=Degree;
            double[] roots;

            if (d > 4 || forcenumeric)
            {

                return NumericFindRoots(goal_error,zerotol);
                /*double xmin, xmax;
                GetRootBounds(out xmin, out xmax);
                SturmSequence seq = new SturmSequence(this);
                List<double> res = new List<double>();
                seq.FindRoots(xmin, xmax, tolerance, res);
                roots = res.ToArray();*/
            }
            else
            {
                //use exact solvers for quartic or simpler equations, which is much faster
                switch(d) {
                    case 1: roots=SolveLinear(cof[1], cof[0]);break;
                    case 2: roots=SolveQuadric(cof[2],cof[1],cof[0]);break;
                    case 3: roots=SolveCubic2(cof[3],cof[2],cof[1],cof[0]);break;
                    case 4: roots=SolveQuartic2(cof[4],cof[3],cof[2],cof[1],cof[0]);break;
                    default: return null; //this happens for 0-degree polynomials with only a constant term
                }
            }

            if (roots==null || roots.Length == 0)
                return null;
            return roots;
        }


        
        public static double[] DerivativeCoefficients(double[] cof)
        {
            int n = cof.Length - 1;
            if (n <= 0)
            { //only constant
                return new double[] { 0.0 };
            }
            double[] newcoefs = new double[n];

            for (int l = 1; l <= n; l++)
                newcoefs[l-1] = cof[l] * l;

            return newcoefs;
        }


        public static double EvalCoefficients(double[] cof, double x)
        {
            //using horners rule
            int i = cof.Length - 1;
            double res = cof[i--];
            while (i >= 0)
                res = res * x + cof[i--];
            return res;
        }


        /*static List<double> InternalFindRoots(double[] cof, double xmin, double xmax, double tol = MathUtil.Epsilon, double dualxtol = MathUtil.Epsilon)
        {

            int degree = cof.Length - 1;

            if (degree < 1)
                throw new Exception("Internal find roots cannot be called with polynomial of degree 1 or less");

            //find search ranges by derivative recursion:
            List<double> roots = new List<double>();

            if (degree == 1)
            {
                //double[] ds = RealPolynomial.SolveQuadric(cof[2], cof[1], cof[0], tol);
                double[] ds = RealPolynomial.SolveLinear(cof[1], cof[0]);
                if (ds==null) return null;
                roots.AddRange(ds);
            }
            else
            {
                List<double> ranges = InternalFindRoots(DerivativeCoefficients(cof), xmin, xmax);
                if (ranges == null) return null;

                double prevx = xmin;
                double root;
                double prevroot = xmax * 2.0;  //use to skip dual roots
                

                foreach (double x in ranges)
                {
                    if (Numerics.Brent(prevx, x, (xev) => { return EvalCoefficients(cof, xev); }, tol, out root))
                    {
                        if (Math.Abs(root - prevroot) > dualxtol)
                            roots.Add(root);
                        prevroot = root;
                    }
                    prevx = x;
                }


                if (Numerics.Brent(prevx, xmax, (xev) => { return EvalCoefficients(cof, xev); }, tol, out root))
                {
                    if (Math.Abs(root - prevroot) > dualxtol)
                        roots.Add(root);
                }


            }

            if (roots.Count == 0)
                return null;

            return roots;
        }*/

        /// <summary>
        /// Finds all the real roots for this polynomial.
        /// If degree is larger than 4, a numeric solver is used internally, otherwise
        /// a direct solver (which is much faster) is used, unless 'forcenumeric' is true.
        /// Tolerance is only used if numeric computation is perfomed.
        /// </summary>
      /*  public double[] FindRoots(double tolerance = MathUtil.Epsilon, bool forcenumeric = false)
        {
            int d = Degree;

            if ((forcenumeric && Degree>2) || Degree>4)
            {
                
                double xmin, xmax;
                GetRootBounds(out xmin, out xmax);
                var rootlist=InternalFindRoots(coefs, xmin, xmax, tolerance);
                if (rootlist != null)
                    return rootlist.ToArray();
                return null;
            }
            else
            {
                //use exact solvers for quartic or simpler equations, which is much faster
                switch (d)
                {
                    case 1: return SolveLinear(coefs[1], coefs[0]); break;
                    case 2: return SolveQuadric(coefs[2], coefs[1], coefs[0]); break;
                    case 3: return SolveCubic(coefs[3], coefs[2], coefs[1], coefs[0]); break;
                    case 4: return SolveQuartic(coefs[4], coefs[3], coefs[2], coefs[1], coefs[0]); break; 
                    default: return null; //this happens for 0-degree polynomials with only a constant term => no solution
                }
            }

        }*/

        /// <summary>
        /// Computes a range, in which all the roots of the polynomial is.
        /// </summary>
        private void GetRootBounds(out double xmin, out double xmax)
        {
            //see http://www.mathsisfun.com/algebra/polynomials-bounds-zeros.html
            double div = cof[Degree];
            double bound1 = 0.0;
            double bound2 = 0.0;

            for (int l = 0; l < cof.Length - 1; l++)
            {
                double c = Math.Abs(cof[l] / div);
                bound1 = Math.Max(bound1, c + 1.0);
                bound2 += c;
            }

            bound2 = Math.Max(1.0, bound2);

            xmax = Math.Min(bound1, bound2);
            xmin = -xmax;
        }
    }

    
    public class SturmSequence
    {
        public static int MaxDepth = 0;

        List<RealPolynomial> polys = new List<RealPolynomial>();
        private double tolerance = 1e-9;

        public SturmSequence(RealPolynomial p)
        {
            RealPolynomial f1 = p.Copy();
            RealPolynomial f2 = f1.Derivative;
            RealPolynomial f3;
            polys.Add(f1);
            polys.Add(f2);

            while (f2.Degree > 0)
            {
                f3 = -(f1 % f2);
                polys.Add(f3);
                f1 = f2;
                f2 = f3;
            }
        }

        public int FindNumberOfSignChanges(double value)
        {
            int numberOfSignChanges = 0;
            double currentValue;
            double oldValue = Math.Round(polys[0].Eval(value), 6);
            double oldSign = Math.Sign(oldValue);
            double currentSign;
            for (int i = 1; i < polys.Count; i++)
            {
                currentValue = Math.Round(polys[i].Eval(value), 6);
                currentSign = Math.Sign(currentValue);
                if (currentSign * oldSign < 0)
                {
                    numberOfSignChanges++;
                }
                if (currentSign != 0)
                {
                    oldSign = currentSign;
                }
            }
            return numberOfSignChanges;
        }

        public int NumberOfRootsInRange(double min, double max)
        {
            //abs if min>max
            return Math.Abs(FindNumberOfSignChanges(min) - FindNumberOfSignChanges(max));
        }



        public override string ToString()
        {
            return string.Join(" | ", polys);
        }

        public void FindRoots(double min, double max, List<double> res, double goal_error=1e-12)
        {
            double error_estimate;
            int numiter;

            int nroot=NumberOfRootsInRange(min, max);
            if (nroot == 0)
                return;
            else if (nroot == 1)
            {
                //single root in range => finalize root finding with a brent solver (normal case)
                //as a special case we can have one root and booth posetive or negative, when the curve
                //just touches the xaxis. Handle this with a min/max search
                double a = polys[0].Eval(min);
                double b = polys[0].Eval(max);
                double e;
                if (a * b <= 0.0)
                {
                    //normal case
                    Numerics.Brent(min, max, (d) => polys[0].Eval(d), out e,goal_error,1e-6);
                }
                else
                { //touching case
                    if (a >= 0.0 && b >= 0.0)
                        e = Numerics.FindMin((d) => polys[0].Eval(d), min, max, tolerance);
                    else
                        e = Numerics.FindMax((d) => polys[0].Eval(d), min, max, tolerance);
                }
                res.Add(e);
            }
            else
            {
                //multiple roots in range
                double diff = max - min;
                if (diff < tolerance)
                    return; //'miss' roots that are to close
                double mid = (max + min) * 0.5;
                FindRoots(min, mid, res,goal_error);
                FindRoots(mid, max, res,goal_error);
            }

        }
    }
}
