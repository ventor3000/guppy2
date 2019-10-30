using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{

    //TODO: finish implementation, or erase this class. (inherit curve)
    public class SuperEllipse2d
    {
        public double power; //power coefficient for super ellipse
        double ratio;   //ratio between major and minor axis, b/a
        Point2d center;
        Vector2d majoraxis;

        public SuperEllipse2d(Point2d center, double radius1, double radius2, double power, double tilt = 0.0)
        {
            //make sure radius1 is larger than radius2 since we will use it for major axis
            if (radius1 < radius2)
            {
                MathUtil.Swap(ref radius1, ref radius2);
                tilt = MathUtil.NormalizeAngle(tilt - MathUtil.Deg90);
            }

            this.power = power;
            this.center = center;
            majoraxis = new Vector2d(Math.Cos(tilt) * radius1, Math.Sin(tilt) * radius1);
            ratio = radius2 / radius1;
        }

        public Transform2d ToStandardPosition
        {
            get
            {
                return Transform2d.Translate(-center.X, -center.Y) * Transform2d.Rotate(-majoraxis.Angle) * Transform2d.Scale(1.0 / majoraxis.Length);
            }
        }

        public Transform2d FromStandardPosition
        {
            get
            {
                return Transform2d.Scale(majoraxis.Length)* Transform2d.Rotate(majoraxis.Angle) * Transform2d.Translate(center.X, center.Y) ;
            }
        }

        public Point2d Eval(double t)
        {
            //see http://en.wikipedia.org/wiki/Superellipse

            //t must be between 0 and pi/2 
            double xsign,ysign;
           // t = MathUtil.NormalizeAngle(t);
            if (t > MathUtil.Deg270)
            {
                xsign = 1.0;
                ysign = -1.0;
            }
            else if (t > MathUtil.Deg180)
            {
                xsign = -1;
                ysign = -1;
            }
            else if (t > MathUtil.Deg90)
            {
                xsign = -1.0;
                ysign = 1.0;
            }
            else
            {
                xsign = 1.0;
                ysign = 1.0;
            }

            //evaluate in standard position and transform
            double p2=2.0/power;
            Point2d pt = new Point2d(
                Math.Pow(Math.Abs(Math.Cos(t)), p2) * xsign,
                Math.Pow(Math.Abs(Math.Sin(t)), p2)*ratio * ysign
            );

            return pt.GetTransformed(FromStandardPosition);
        }
    }
}
