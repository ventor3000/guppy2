using Guppy2.Calc.Geom2d;
using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2Test
{
    public class EntityLine:EntityCurve
    {

        Line2d geometry;

        static EntityLine()
        {
        }

        public EntityLine(double x1, double y1, double x2, double y2)
        {
            geometry=new Line2d(x1, y1, x2, y2);
        }
        

        static Entity MakeLine(Dictionary<string, object> data)
        {
            return null;
        }

        public override void Create(Entity ent, Dictionary<string, object> data)
        {
            base.Create(ent, data);
        }

        public override void Draw(Painter painter)
        {

            painter.Color = PhysicalColor;

            painter.DrawLineT(geometry.X1, geometry.Y1, geometry.X2, geometry.Y2);
        }
    }
}
