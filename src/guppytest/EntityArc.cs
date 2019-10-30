using Guppy2.Calc.Geom2d;
using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2Test
{
    public class EntityArc:EntityCurve
    {

        Arc2d geometry;

        static EntityArc()
        {
        }

        public EntityArc(double x1, double y1, double x2, double y2,double bulge)
        {
            geometry=new Arc2d(new Point2d(x1,y1),new Point2d(x2,y2),bulge);
        }
        
        public override void Draw(Painter painter)
        {
            painter.Color = PhysicalColor;
            painter.DrawArcT(geometry.Start.X, geometry.Start.Y, geometry.End.X, geometry.End.Y,geometry.Bulge);
            
        }

    }
}
