using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{
    /// <summary>
    /// A class for storing points, where no point is closer to another than a given tolerance,
    /// that is, each point is geometrically unique.
    /// </summary>
    public class Point2dSet
    {
        private List<Point2d> pts = new List<Point2d>(); //all points in set.
        private double sqrtol; //tolerance, squared

        public Point2dSet(double uniquetolerance=MathUtil.Epsilon)
        {
            sqrtol = MathUtil.Square(uniquetolerance);
        }

        public bool Add(Point2d p)
        {
            //TODO: optimize a bit, better than linear search could be done
            foreach (Point2d pt in pts)
            {
                if (p.SquaredDistance(pt) <= sqrtol)
                    return false; //already existed
            }

            pts.Add(p);
            return true;
        }

        public Point2d[] ToArray()
        {
            if (pts.Count == 0)
                return null;
            return pts.ToArray();
        }        

        public void Transform(Transform2d t) {
            for (int l = 0; l < pts.Count;l++ )
                pts[l] = pts[l].GetTransformed(t);
        }

        public void InverseTransform(Transform2d t)
        {
            t = t.Inversed;
            Transform(t);
        }

        public int Count
        {
            get
            {
                return pts.Count;
            }
        }



        public void AddRange(IEnumerable<Point2d> pts)
        {
            if (pts == null)
                return;
            foreach (Point2d pt in pts)
            {
                if (pt != null)
                    Add(pt);
            }
        }
    }
}
