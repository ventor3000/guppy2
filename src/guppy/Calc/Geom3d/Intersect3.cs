using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom3d
{
    public class Intersect3
    {
        private static double[] empty_array=new double[0];
        private static Point3[] empty_pt_array = new Point3[0];

        private static double[] RealArray(params double[] reals) { return reals; }
        private static Point3[] Point3Array(params Point3[] pts) { return pts; }


        public static double[] RayPlaneParametric(Ray3 ray, Plane3 plane,double tol=MathUtil.Epsilon)
        {
            Vector3 u = ray.Direction;
            Vector3 w = ray.Position - plane.Origo;

            double d = plane.Normal.Dot(u);
            double n = -plane.Normal.Dot(w);

            if (Math.Abs(d) < tol)
                return empty_array; //parallel

            return RealArray(n / d);
        }


        public static Point3 RayPlane(Ray3 ray, Plane3 plane)
        {
            double[] t = RayPlaneParametric(ray, plane);
            if (t.Length == 0)
                return null;
            return ray.Eval(t[0]);
        }


        static Line3 PlanePlaneIntersection(Plane3 plane1,Plane3 plane2,double tol=MathUtil.Epsilon)
        {
            // get direction of intersection line
            Vector3 intdir = plane1.Normal.Cross(plane2.Normal);

            //Next is to calculate a point on the line to fix it's position. This is done by finding a vector from
            //the plane2 location, moving parallel to it's plane, and intersecting plane1. To prevent rounding
            //errors, this vector also has to be perpendicular to lineDirection. To get this vector, calculate
            //the cross product of the normal of plane2 and the lineDirection.     
            Vector3 ldir = plane2.Normal.Cross(intdir);

            double numerator = plane1.Normal.Dot(ldir);
            
            // prevent divide by zero (parallel planes)
            if (Math.Abs(numerator) > tol)
            {
                Vector3 plane1ToPlane2 = plane1.Origo - plane2.Origo;
                double t = plane1.Normal.Dot(plane1ToPlane2) / numerator;
                Point3 intpoint = plane2.Origo + t * ldir;

                return new Line3(intpoint, intpoint + intdir);
            }

            return null;
        }


        
        

    }
}
