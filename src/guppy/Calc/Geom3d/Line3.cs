using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom3d
{
    public class Line3
    {
        public Point3 Start;
        public Point3 End;

        public Line3(Point3 start, Point3 end)
        {
            this.Start = start;
            this.End = end;
        }

        
    }
}
