using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom3d
{
    public class Sphere3
    {
        public double Radius;
        public Point3 Center;

        public Sphere3(Point3 center, double radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        public Sphere3(double x, double y, double z, double radius)
        {
            this.Center = new Point3(x, y, z);
            this.Radius = radius;
        }


        public double X { get { return Center.X; } }
        
        public double Y { get { return Center.Y; } }
        
        public double Z { get { return Center.Z; } }


        public override string ToString()
        {
            return StringUtil.FormatReals(X, Y, Z, Radius);
        }

    
    }
}
