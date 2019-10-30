using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom3d
{
    public class Ray3
    {
        public Point3 Position;
        public Vector3 Direction;

        public Ray3(Point3 pos, Vector3 dir)
        {
            this.Position = pos;
            this.Direction = dir;
        }


        public Ray3 Transform(Transform3 t)
        {
            return new Ray3(Position.Transform(t), Direction.Transform(t));
        }


        public Point3 Eval(double t)
        {
            return new Point3(
                Position.X + Direction.X * t,
                Position.Y + Direction.Y * t,
                Position.Z + Direction.Z * t
            );

        }
    }
}
