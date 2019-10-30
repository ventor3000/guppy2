using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc
{
    public static class MathUtil
    {
        // TODO: NthRoot, IsEven, IsOdd, gcd, lcd etc

        public const double Epsilon = 1e-6;
        public const double PI = Math.PI;
        public const double Deg360 = PI * 2.0;
        public const double Deg270 = PI * 1.5;
        public const double Deg180 = PI;
        public const double Deg90 = PI * 0.5;
        public const double Deg60 = Deg180 / 3.0;
        public const double Deg45 = PI * 0.25;

        private const double rad2deg = 180.0 / PI; //ised by DegToRad / RadToDeg

        public static bool IsZero(double d, double tol = Epsilon) { return Math.Abs(d) <= tol; }
        public static bool Equals(double a, double b, double tol = Epsilon) { return Math.Abs(b - a) <= tol; }
        public static double DegToRad(double deg) { return deg / rad2deg; }
        public static double RadToDeg(double rad) { return rad * rad2deg; }

        
        public static void Swap<T>(ref T a, ref T b) { T tmp = a; a = b; b = tmp; }

        public static bool Sort(ref double v1, ref double v2)
        {
            if (v1 > v2)
            {
                Swap(ref v1, ref v2);
                return true;
            }
            return false;
        }

        public static bool Sort(ref int v1, ref int v2)
        {
            if (v1 > v2)
            {
                Swap(ref v1, ref v2);
                return true;
            }
            return false;
        }


        public static double NormalizeAngle(double a)
        {
            if (a >= 0)
                return a % Deg360;
            else
                return Deg360 + (a % Deg360);
        }

        public static double CubeRoot(double v)
        {
            if (v < 0.0)
                return -(Math.Pow(-v, 1.0 / 3.0));
            else
                return Math.Pow(v, 1.0 / 3.0);
        }

        /// <summary>
        /// Solves the equation ax^2+bx+c. Returns true if it was solvable. If not solvable,
        /// x1 and y1 is set to Nan. The results x1 is always less or equal to x2, ie result are sorted.
        /// </summary>
        public static bool SolveQuadratic(double a, double b, double c, out double x1, out double x2)
        {
            // Quadratic Formula: x = (-b +- sqrt(b^2 - 4ac)) / 2a
            // TODO: not working if a is zero

            // Calculate the inside of the square root
            double inroot = (b * b) - 4 * a * c;

            if (inroot < 0)
            {
                //There is no solution
                x1 = double.NaN;
                x2 = double.NaN;
                return false;
            }
            else
            {
                // Compute the value of each x
                // if there is only one solution, both x's will be the same
                double s = Math.Sqrt(inroot);
                x1 = (-b - s) / (2.0 * a);
                x2 = (-b + s) / (2.0 * a);
                return true;
            }
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

        /// <summary>
        /// Returns an array with the real solutions of euqtion ax^2+bx+c=0
        /// This array can be 1 or 2 elements long. null is returned on no solutions.
        /// </summary>
        public static double[] SolveQuadric(double a, double b, double c, double zerotolerance = 0.0)
        {

            double[] s = null;

            double p, q, d;


            if (MathUtil.IsZero(a))
                return SolveLinear(b, c);

            p = b / (2.0 * a);
            q = c / a;

            d = p * p - q;

            if (MathUtil.IsZero(d))
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
        /// Solves the cubic c3*x^3+c2*x^2+c1*x+c0=0, returning an array with the solutions.
        /// Null is returned if there are no solutions, which should never happen for 3:d degree
        /// polynomials.
        /// </summary>
        public static double[] SolveCubic(double c3, double c2, double c1, double c0, double tol = 1e-9)
        {
            // see Graphics Gems I book

            int i, num;
            double sub;
            double A, B, C;
            double sq_A, p, q;
            double cb_p, D;
            double[] s = new double[3];   //solutions


            if (MathUtil.IsZero(c3, 1e-9))
                return SolveQuadric(c2, c1, c0, tol);

            /* normal form: x^3 + Ax^2 + Bx + C = 0 */

            A = c2 / c3;
            B = c1 / c3;
            C = c0 / c3;

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
                s[1] = -t * Math.Cos(phi + Deg60);
                s[2] = -t * Math.Cos(phi - Deg60);
                num = 3;
            }
            else /* one real solution */
            {
                double sqrt_D = Math.Sqrt(D);
                double u = CubeRoot(sqrt_D - q);
                double v = -CubeRoot(sqrt_D + q);

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
        /// Returns the minimum included angle between two given angles using a sweep direction.
        /// </summary>
        /// <param name="fromangle">From angle.</param>
        /// <param name="toangle">To angle.</param>
        /// <param name="ccw">If the sweepangle is ccw, otherwise its cw.</param>
        /// <returns>The sweepangle between the two angles.</returns>
        public static double DeltaAngle(double fromangle, double toangle, bool ccw)
        {
            double a1 = NormalizeAngle(fromangle);
            double a2 = NormalizeAngle(toangle);

            //make sure a1<a2 if ccw, else that a2<a1
            if (ccw && a1 > a2)
                a1 -= Deg360;
            else if (!ccw && a2 > a1)
                a2 -= Deg360;

            return ccw ? a2 - a1 : a1 - a2;
        }

        public static bool IsAngleBetween(double startang, double endang, double queryang, bool ccw)
        {

            double c0 = Math.Cos(startang);
            double c1 = Math.Cos(endang);
            double cp = Math.Cos(queryang);
            double s0 = Math.Sin(startang);
            double s1 = Math.Sin(endang);
            double sp = Math.Sin(queryang);
            double s = (c0 - cp) * s1 + (sp - s0) * c1 + cp * s0 - sp * c0;

            if (ccw)
                return s <= 0.0;
            else
                return s >= 0.0;
        }

        public static double Square(double v) {return v * v;}
        public static double Hypot(double dx, double dy) {return Math.Sqrt(dx * dx + dy * dy);}
        public static double Clamp(double v, double min, double max) { if (v < min) return min; if (v > max) return max; return v; }
        public static bool IsBetween(double v, double min, double max) { return v >= min && v <= max; }
        
        public static double Sec(double v) { return 1.0 / Math.Cos(v); }    //secant
        public static double Csc(double v) { return 1.0 / Math.Sin(v); }    //cosecant
        public static double Cot(double v) { return 1.0 / Math.Tan(v); }    //cotangent
        
        public static double ArcCos(double v) {return Math.Acos(Clamp(v,-1.0,1.0));}  //TODO: throw on very out of range values

        /// <summary>
        /// Returns the double value that is the smallest possible value
        /// that is larger than a given value.
        /// </summary>
        public unsafe static double Inc(double v)
        {
            ulong* lv = (ulong*)&v;
            if ((*lv & 0x7FF0000000000000) == 0x7FF0000000000000)
            {
                if (double.IsNegativeInfinity(v)) return double.MinValue;
                return v; // inf or nan
            }
            if ((*lv & 0x8000000000000000) != 0)    //negative
            {
                if (*lv == 0x8000000000000000) //negative zero
                {
                    *lv = 1; //1 is after 0
                    return v;
                }
                --*lv;
            }
            else
            {   // Positive number
                ++*lv;
            }
            return v;
        }

        public static double Dec(double v)
        {
            return -Inc(-v);
        }


        
        
    }
}
