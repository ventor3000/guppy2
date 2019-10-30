using Guppy2.Calc.Geom2d;
using Guppy2.GUI.ExtraWidgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2Test
{
    public class CommandTest
    {
        public static void Run()
        {
            Ellipse2d elp1 = new Ellipse2d(new Point2d(0, 0), 10.0, 10.0);
            Ellipse2d elp2 = new Ellipse2d(new Point2d(0, 0), 20.0, 10.0);

            GeneralConic2d con1 = elp1.ToGeneralConic();
            GeneralConic2d con2 = elp2.ToGeneralConic();

            Intersect2d.ConicConic(con1, con2);

        }
    }
}
