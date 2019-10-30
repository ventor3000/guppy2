using Guppy2.AppUtils;
using Guppy2.Calc.Geom2d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2Test
{
    public class CommandArc
    {
        public static void Run()
        {
            Point2d p1=TC.GetPoint(LTF._("Specify first point"));
            Point2d p2 = TC.GetPoint(LTF._("Specify second point"));


            EntityArc a = new EntityArc(p1.X, p1.Y, p2.X, p2.Y, 0.5);
            TC.AddEntity(a);

        }
    }
}
