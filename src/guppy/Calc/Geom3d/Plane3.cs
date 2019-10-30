using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom3d
{
    public class Plane3
    {
        public Point3 Origo;
        public Vector3 Normal;

        public Plane3(Point3 origo, Vector3 normal)
        {
            this.Origo = origo;
            this.Normal = normal.Normalized;
        }

        public Plane3()
        {
            Origo = Point3.Origo;
            Normal = Vector3.ZAxis;
        }

        public bool Contains(Point3 p, double tol=MathUtil.Epsilon)
        {
            return Math.Abs((p - Origo).Dot(Normal)) < tol;
        }

        /*public Location Location(Point3 pt, double tol=MathUtil.Epsilon)
        {
            Vector3 ptvec = (pt - Origo).Normalized;
            double dot = ptvec.Dot(Normal);
            if (dot < -tol)
                return Geom3d.Location.Inside;
            else if (dot > tol)
                return Geom3d.Location.Outside;
            return Geom3d.Location.On;
        }*/

        
        
        
    }





    /*
     * static bool planePlaneIntersection(out Point3d linePoint, out Vector3d lineVec, Plane3d plane1, Plane3d plane2)
        {

            linePoint = null;

            lineVec = null;



            //Get the normals of the planes.

            Vector3d plane1Normal = plane1.Normal;

            Vector3d plane2Normal = plane2.Normal;



            //We can get the direction of the line of intersection of the two planes by calculating the

            //cross product of the normals of the two planes. Note that this is just a direction and the line

            //is not fixed in space yet.

            lineVec = plane1Normal.Cross(plane2Normal);



            //Next is to calculate a point on the line to fix it's position. This is done by finding a vector from

            //the plane2 location, moving parallel to it's plane, and intersecting plane1. To prevent rounding

            //errors, this vector also has to be perpendicular to lineDirection. To get this vector, calculate

            //the cross product of the normal of plane2 and the lineDirection.     

            Vector3d ldir = plane2Normal.Cross( lineVec);



            double numerator = plane1Normal.Dot(ldir);



            //Prevent divide by zero.

            if (Math.Abs(numerator) > Calc.Epsilon)
            {
                Vector3d plane1ToPlane2 = plane1.Origo - plane2.Origo;

                double t = plane1Normal.Dot( plane1ToPlane2) / numerator;

                linePoint = plane2.Origo + t * ldir;

                return true;

            }

            return false;
        }
     * */


    
    
        
   /*

    /// <summary>
    /// Returns a parameter where the plane intersects a line. This parameter is 0.0-1.0 if the 
    /// plane is between the points. If the intersection is out of this range a 
    /// value smaller than 0.0 or larger than 1.0 is gotten.
    /// </summary>
    /// <param name="line">The line to intersect this plane with.</param>
    /// <param name="res">Set to the point of intersection between this plane.</param>
    /// <returns>true if the line intersects this plane, otherwise false (which means the line is parallel with this plane).</returns>
        public bool Intersect(Line3d line,out Point3d res) {
            double si;
            Point3d s=line.Start;
            Point3d e=line.End;
      res = null;

            if(!ParametricIntersect(s,e,out si))
                return false;
        
            res=s + (e-s)*si; // compute segment intersect point
            return true;
        }

    /// <summary>
    /// True if the point is on the plane, within a given tolerance.
    /// </summary>
    public bool PointInPlane(Point3d point, double tol)
    {
      return Math.Abs((point - origo).Dot(normal)) < tol;
    }

    public Point3d Origo
    {
      get
      {
        return new Point3d(origo);
      }
    }

    public Vector3d Normal
    {
      get
      {
        return new Vector3d(normal);
      }
    }


    /// <summary>
    /// Computes a side of a point relative the plane. If the point is on
    /// the same side as the normal points to, Side.Right is returned.
    /// If the point is within 'tol' tolerance on the plane (in a dot product) Side.On is returned.
    /// Else Side.Left is returned.
    /// </summary>
    /// <param name="pt"></param>
    /// <param name="tol"></param>
    /// <returns></returns>
    public Side Side(Point3d pt,double tol)
    {
      Vector3d ptvec = pt - Origo;
      if (ptvec.Length < Calc.Epsilon)
          return Genamo.Side.On;
      ptvec.Normalize();
      double dot=ptvec.Dot(Normal);
      if (dot < -tol) //angle>90 degrees
        return Genamo.Side.Left;
      else if(dot>tol)
        return Genamo.Side.Right;
      return Genamo.Side.On;
    }

    public Point3d ClosestPoint(Point3d point)
    {
        Line3d l = new Line3d(point, point + normal);
        Point3d res;
        Intersect3d.PlaneLine(this, l, out res);
        return res;
    }

    public double D
    {
        get
        {
            return normal.Dot(new Vector3d(origo.x, origo.y, origo.z));
        }
    }


        Point3d origo;
        Vector3d normal;
    }
     * */


/*
 * public Matrix3d ArbitraryAxis(Vector3d nz, double elevation)
        {
            double s = 1.0 / 64.0;
            Vector3d nx;

            if (Math.Abs(nz.x) < s && Math.Abs(nz.y) < s)
                nx = Vector3d.YAxis.Cross(nz);
            else
                nx = Vector3d.ZAxis.Cross(nz);

            nx.Normalize();

            Vector3d ny = nz.Cross(nx);

            ny.Normalize();

            Matrix3d res = new Matrix3d();
            res.XAxis = nx;
            res.YAxis = ny;
            res.ZAxis = nz;


            if (elevation != 0.0)
            {
                res = Matrix3d.Translate(0.0, 0.0, elevation) * res;
            }

            return res;
        }
 * */
}
