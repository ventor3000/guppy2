using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{
    public abstract class Curve2d
    {
        public abstract bool IsClosed { get; }
        public abstract double Area { get; }
        public abstract double Length { get; }
        
        public abstract Point2d PointAt(double t);
        public abstract void Reverse();
        public abstract Vector2d Tangent(Point2d where);
        public abstract Point2d ClosestPoint(Point2d from);
        public abstract double PositionOf(Point2d pnt);
        public abstract double Distance(Point2d p);
        public abstract bool Transform(Transform2d t);
        public abstract Vector2d DirectionAt(double t);
    }
}
