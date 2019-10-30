using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc
{
    public static class Numerics
    {


        /// <summary>
        /// Computes a Brent-style root bracketing. Returns true on success, or false on failure.
        /// Failure can only happen if the function value at xmin and xmax has the same sign, 
        /// and the function values at xmin/xmax is not within zero tolerance.
        /// </summary>
        /// <param name="xmin">Lower bound for root search.</param>
        /// <param name="xmax">Upper bound for root search.</param>
        /// <param name="function">The function to evaluate.</param>
        /// <param name="res">The x value for the root.</param>
        /// <param name="tol">The iteration stops when this tolerance is reached. If never reached it stops after 1000 iterations.</param>
        /// <param name="zerotol">If abs(funtion(xmin or xmax)) xmin/xmax is smaller than this value and same signs for xmin/xmax is sent, it is reported as success.
        /// This is to find roots close to 0 but with a small tolerance error. Set to negative to disable.</param>
        /// <returns></returns>
        public static bool Brent(double xmin, double xmax, Func<double, double> function, out double res,double tol,double zerotol)
        {
            double a = xmin;
            double b = xmax;
            double c = 0.0;
            double d = double.MaxValue;

            double fa = function(a);
            double fb = function(b);

            double fc = 0;
            double s = 0;
            double fs = 0;

            // if f(a) f(b) >= 0 then error-exit
            if (fa * fb >= 0)
            {
                if (Math.Abs(fa) < Math.Abs(fb))
                {
                    res = a;
                    return Math.Abs(fa) <= zerotol;
                }
                else
                {
                    res = b;
                    return Math.Abs(fb) <= zerotol;
                }
            }

            // if |f(a)| < |f(b)| then swap (a,b) end if
            if (Math.Abs(fa) < Math.Abs(fb))
            { double tmp = a; a = b; b = tmp; tmp = fa; fa = fb; fb = tmp; }

            c = a;
            fc = fa;
            bool mflag = true;
            int i = 0;

            while (!(fb == 0) && (Math.Abs(a - b) > tol))
            {
                if ((fa != fc) && (fb != fc))
                    // Inverse quadratic interpolation
                    s = a * fb * fc / (fa - fb) / (fa - fc) + b * fa * fc / (fb - fa) / (fb - fc) + c * fa * fb / (fc - fa) / (fc - fb);
                else
                    // Secant Rule
                    s = b - fb * (b - a) / (fb - fa);

                double tmp2 = (3 * a + b) / 4;
                if ((!(((s > tmp2) && (s < b)) || ((s < tmp2) && (s > b)))) || (mflag && (Math.Abs(s - b) >= (Math.Abs(b - c) / 2))) || (!mflag && (Math.Abs(s - b) >= (Math.Abs(c - d) / 2))))
                {
                    s = (a + b) / 2;
                    mflag = true;
                }
                else
                {
                    if ((mflag && (Math.Abs(b - c) < tol)) || (!mflag && (Math.Abs(c - d) < tol)))
                    {
                        s = (a + b) / 2;
                        mflag = true;
                    }
                    else
                        mflag = false;
                }
                fs = function(s);
                d = c;
                c = b;
                fc = fb;
                if (fa * fs < 0) { b = s; fb = fs; }
                else { a = s; fa = fs; }

                // if |f(a)| < |f(b)| then swap (a,b) end if
                if (Math.Abs(fa) < Math.Abs(fb))
                {
                    MathUtil.Swap(ref a, ref b);
                    MathUtil.Swap(ref fa, ref fb);
                }
                if (++i > 1000) //we have'nt reached  requested tolerance within 1000 iterations, stop now, we wont do better
                    break;
            }

            res = b;
            return true;
        }

        
        /*
        public static bool Bisect(double a,double b,Func<double, double> f,double tol,out double c)
        {
            double fa = f(a);
            double fb = f(b);
            c = 0.0;


            for (int l = 0; l < 1000; l++)
            {
                c = (b + a) * 0.5;
                double fc = f(c);
                if (Math.Abs(fc) <=tol)
                    return true;

                if (fa * fc > 0.0)
                {
                    a = c;
                    fa = fc;
                }
                else
                {
                    b = c;
                    fb = fc;
                }
            }
            
            return false;
        }*/


        public static double FindMin(Func<double,double> fn,double xa,double xb,double tolerance) {
            // Uses the golden search method from the book "Practical numerical methods with C#", page 225

            double x1,x2,f1,f2;
            double g = 1.0 - (Math.Sqrt(5.0) - 1.0) / 2.0;
            x1=xa+g*(xb-xa);
            x2=xb-g*(xb-xa);
            f1=fn(x1);
            f2=fn(x2);

            do {
                if(f1<f2) {
                    xb=x2;
                    x2=x1;
                    x1=xa+g*(xb-xa);
                    f2=f1;
                    f1=fn(x1);
                }
                else {
                    xa=x1;
                    x1=x2;
                    x2=xb-g*(xb-xa);
                    f1=f2;
                    f2=fn(x2);
                }

                

            } while(Math.Abs(xb-xa)>tolerance);
            
            return (xa+xb)*0.5;
        }

        public static double FindMax(Func<double, double> fn, double xa, double xb, double tolerance)
        {
            return FindMin((x) => -fn(x), xa, xb, tolerance);
        }


        /// <summary>
        /// Internal helper for a simpson integration refinement
        /// </summary>
        /// <param name="func"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="n"></param>
        /// <param name="res"></param>
        static private void IntegrateTrapezoid(Func<double,double> func, double a, double b, int n, ref double res)
        {
            double x, tnm, sum, del;
            int it, j;
            if (n == 1)
            {
                res = 0.5 * (b - a) * (func(a) + func(b));
            }
            else
            {
                for (it = 1, j = 1; j < n - 1; j++)
                    it <<= 1;
                tnm = it;
                del = (b - a) / tnm; //This is the spacing of the points to be added.
                x = a + 0.5 * del;
                for (sum = 0.0, j = 1; j <= it; j++, x += del)
                    sum += func(x);
                res = 0.5 * (res + (b - a) * sum / tnm); //This replaces res by its refined value.
                return;
            }
        }

        /// <summary>
        /// Returns the integral of the function func from a to b. 
        /// Integration is performed by Simpson’s rule.
        /// </summary>
        /// <param name="func">The function to integrate.</param>
        /// <param name="a">Start x of function.</param>
        /// <param name="b">End x of function.</param>
        /// <param name="maxiter">The maximum number of iterations. This can be usually
        /// be a large number since the iteration stops when the given tolrance has been reached.</param>
        /// <param name="tol">The tolerance wanted. Lower number will result in a higher number of iterations.</param>
        /// <returns>The integral of the function.</returns>
        static public double Integrate(Func<double,double> func, double a, double b, int maxiter, double tol)
        {
            int j;
            double s, st = 0.0, ost = 0.0, os = 0.0;
            for (j = 1; j <= maxiter; j++)
            {
                IntegrateTrapezoid(func, a, b, j, ref st);
                s = (4.0 * st - ost) / 3.0;
                if (j > 5) //Avoid spurious early convergence.
                    if (Math.Abs(s - os) < tol * Math.Abs(os) || (s == 0.0 && os == 0.0))
                        return s;
                os = s;
                ost = st;
            }
            throw new Exception("Numeric integration failed to converge");
        }

    }
}
