using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom3d
{
    public class TanTanTanSphere
    {

        public static Sphere3 PointPointPointPoint(Point3 p1, Point3 p2, Point3 p3, Point3 p4)
        {

            throw new NotImplementedException();

            /* this code does not work correctly
            // See http://2000clicks.com/mathhelp/GeometryConicSectionSphereEquationGivenFourPoints.aspx

            double a1 = p1.X, a2 = p1.Y, a3 = p1.Z;
            double b1 = p2.X, b2 = p2.Y, b3 = p2.Z;
            double c1 = p3.X, c2 = p3.Y, c3 = p3.Z;
            double d1 = p4.X, d2 = p4.Y, d3 = p4.Z;


            double f1 = ((a1 * a1 + a2 * a2 + a3 * a3) * (c2 * d3 + d2 * b3 + b2 * c3 - d2 * c3 - b2 * d3 - c2 * b3) +
                (b1 * b1 + b2 * b2 + b3 * b3) * (d2 * c3 + c2 * a3 + a2 * d3 - c2 * d3 - a2 * c3 - d2 * a3) +
                (c1 * c1 + c2 * c2 + c3 * c3) * (a2 * b3 + b2 * d3 + d2 * a3 - b2 * a3 - d2 * b3 - a2 * d3) +
                (d1 * d1 + d2 * d2 + d3 * d3) * (b2 * a3 + a2 * c3 + c2 * b3 - a2 * b3 - c2 * a3 - b2 * c3)) /
                (2 * (a1) * (c2 * d3 + d2 * b3 + b2 * c3 - d2 * c3 - b2 * d3 - c2 * b3) +
                (b1) * (d2 * c3 + c2 * a3 + a2 * d3 - c2 * d3 - a2 * c3 - d2 * a3) +
                (c1) * (a2 * b3 + b2 * d3 + d2 * a3 - b2 * a3 - d2 * b3 - a2 * d3) +
                (d1) * (b2 * a3 + a2 * c3 + c2 * b3 - a2 * b3 - c2 * a3 - b2 * c3));


            double f2 = ((a2 * a2 + a3 * a3 + a1 * a1) * (c3 * d1 + d3 * b1 + b3 * c1 - d3 * c1 - b3 * d1 - c3 * b1) +
                (b2 * b2 + b3 * b3 + b1 * b1) * (d3 * c1 + c3 * a1 + a3 * d1 - c3 * d1 - a3 * c1 - d3 * a1) +
                (c2 * c2 + c3 * c3 + c1 * c1) * (a3 * b1 + b3 * d1 + d3 * a1 - b3 * a1 - d3 * b1 - a3 * d1) +
                (d2 * d2 + d3 * d3 + d1 * d1) * (b3 * a1 + a3 * c1 + c3 * b1 - a3 * b1 - c3 * a1 - b3 * c1)) /
                       (2 * (a2) * (c3 * d1 + d3 * b1 + b3 * c1 - d3 * c1 - b3 * d1 - c3 * b1) +
                         (b2) * (d3 * c1 + c3 * a1 + a3 * d1 - c3 * d1 - a3 * c1 - d3 * a1) +
                         (c2) * (a3 * b1 + b3 * d1 + d3 * a1 - b3 * a1 - d3 * b1 - a3 * d1) +
                         (d2) * (b3 * a1 + a3 * c1 + c3 * b1 - a3 * b1 - c3 * a1 - b3 * c1));

            double f3 = ((a3 * a3 + a1 * a1 + a2 * a2) * (c1 * d2 + d1 * b2 + b1 * c2 - d1 * c2 - b1 * d2 - c1 * b2) +
                (b3 * b3 + b1 * b1 + b2 * b2) * (d1 * c2 + c1 * a2 + a1 * d2 - c1 * d2 - a1 * c2 - d1 * a2) +
                (c3 * c3 + c1 * c1 + c2 * c2) * (a1 * b2 + b1 * d2 + d1 * a2 - b1 * a2 - d1 * b2 - a1 * d2) +
                (d3 * d3 + d1 * d1 + d2 * d2) * (b1 * a2 + a1 * c2 + c1 * b2 - a1 * b2 - c1 * a2 - b1 * c2)) /
                       (2*(a3) * (c1 * d2 + d1 * b2 + b1 * c2 - d1 * c2 - b1 * d2 - c1 * b2) +
                         (b3) * (d1 * c2 + c1 * a2 + a1 * d2 - c1 * d2 - a1 * c2 - d1 * a2) +
                         (c3) * (a1 * b2 + b1 * d2 + d1 * a2 - b1 * a2 - d1 * b2 - a1 * d2) +
                         (d3) * (b1 * a2 + a1 * c2 + c1 * b2 - a1 * b2 - c1 * a2 - b1 * c2));


            Point3 center=new Point3(f1,f2,f3);
            double rad=center.Distance(p1);
            return new Sphere3(center,rad);
            */

        }
    }
}
