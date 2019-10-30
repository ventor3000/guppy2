using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom3d
{
    public class Point3
    {
        public readonly double X;
        public readonly double Y;
        public readonly double Z;

        public static readonly Point3 Origo=new Point3(0,0,0);

        public Point3(double x, double y,double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }


        public double Distance(Point3 p)
        {
            double dx = p.X - X;
            double dy = p.Y - Y;
            double dz = p.Z - Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static Vector3 operator -(Point3 p1, Point3 p2)
        {
            //returns a vector from this point to p
            return new Vector3(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        public static Point3 operator +(Point3 p, Vector3 v)
        {
            return new Point3(p.X + v.X, p.Y + v.Y, p.Z + v.Z);
        }

        public static Point3 operator -(Point3 p, Vector3 v)
        {
            return new Point3(p.X - v.X, p.Y - v.Y, p.Z - v.Z);
        }

        public static Point3 operator *(Point3 pt, double s)
        {
            return new Point3(pt.X * s, pt.Y * s, pt.Z * s);
        }

        public static Point3 operator /(Point3 pt, double s)
        {
            return new Point3(pt.X / s, pt.Y / s, pt.Z / s);
        }
        
        public override string ToString()
        {
            return StringUtil.FormatReal(X) + " , " + StringUtil.FormatReal(Y) + " , " + StringUtil.FormatReal(Z);
        }

        public static Point3 operator +(Point3 a, Point3 b)
        {
            return new Point3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public Vector3 ToVector()
        {
            return new Vector3(X, Y, Z);
        }

        public Point3 Transform(Transform3 t)
        {
            double tx,ty,tz;
            t.Apply(X, Y, Z, out tx, out ty, out tz,true);
            return new Point3(tx, ty, tz);
        }
        
    }
}
