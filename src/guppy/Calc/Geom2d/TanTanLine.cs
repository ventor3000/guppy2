using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{
    public static class TanTanLine
    {
        /// <summary>
        /// Returns an array of two points, or null if tangent not possible.
        /// Never returns a single point, even if booth points are equal.
        /// </summary>
        public static Point2d[] CirclePoint(Circle2d ci, Point2d pt)
        {
            Circle2d tc=Circle2d.From2Points(ci.Center, pt);
            Point2d[] pts=Intersect2d.CircleCircle(tc, ci);
            if (pts == null) return null;
            if (pts.Length == 1) { Array.Resize(ref pts, 2); pts[1] = pts[0]; }
            return pts;
        }


        public static Line2d[] CircleCircle(Circle2d ci1, Circle2d ci2)
        {
            // see http://en.wikipedia.org/wiki/Tangent_lines_to_circles#Algebraic_solutions
            // implemented by Robert.P. 2013-02-06

            List<Line2d> res = null;

            double k = 1;
            double d = ci1.Center.Distance(ci2.Center);
            if (d < MathUtil.Epsilon)
                return null;
            
            double r1 = -ci1.Radius;
            double r2 = -ci2.Radius;

            double dx = (ci2.X - ci1.X)/d;
            double dy = (ci2.Y - ci1.Y)/d;
            

            for (int side1 = 0; side1 < 2; side1++)
            {
                for (int side2 = 0; side2 < 2; side2++)
                {

                    double r = (r2 - r1) / d;
                    double srt = 1 - r * r;
                    if (srt < 0.0) continue;
                    srt = Math.Sqrt(srt);

                    double a = r * dx - k * dy * srt;
                    double b = r * dy + k * dx * srt;
                    double c = r1 - (a * ci1.X + b * ci1.Y);

                    if (res == null) res = new List<Line2d>();
                    
                    res.Add(new Line2d(ci1.X - a * r1, ci1.Y - b * r1, ci2.X - a * r2 , ci2.Y - b * r2));

                    r1 = -r1; //swap side for circle 1
                }
                r2 = -r2; //swap side for circle 2
            }

            if (res == null)
                return null;
            return res.ToArray();
            
            
        }
      
       

        
    }
}
