using Guppy2.Calc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{
    public class Vector2d
    {
        public readonly double X;
        public readonly double Y;

        public Vector2d(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public static Vector2d FromAngle(double a)
        {
            return new Vector2d(Math.Cos(a), Math.Sin(a));
        }

        public static Vector2d FromSlope(double slope)
        {
            return FromAngle(Math.Atan(slope));
        }
        
        public static Vector2d FromAngleAndLength(double ang,double len)
        {
            return new Vector2d(Math.Cos(ang)*len, Math.Sin(ang)*len);
        }
        

        public double Length
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y);
            }
        }

        public double SquaredLength
        {
            get
            {
                return X * X + Y * Y;
            }
        }

        public double Angle
        {
            get
            {
                double a = Math.Atan2(Y, X);
                if (a < 0.0) a += MathUtil.Deg360;
                return a;
            }
        }

        public Vector2d GetTransformed(Transform2d t)
        {
            double tx, ty;
            t.Apply(X, Y, out tx, out ty, false);
            return new Vector2d(tx, ty);
        }

        public Vector2d Normalized
        {
            get
            {
                double m = Length;
                if (m == 0.0)
                    return null;
                double x = X / m;
                double y = Y / m;
                return new Vector2d(x, y);
            }
        }

        public Vector2d Rotated90 { get { return new Vector2d(-Y, X); } }
        public Vector2d Rotated180 { get { return new Vector2d(-X, -Y); } }
        public Vector2d Rotated270 { get { return new Vector2d(Y, -X); } }
        


        public static Vector2d operator *(Vector2d v, double n)
        {
            return new Vector2d(v.X * n, v.Y * n);
        }

        public static Vector2d operator /(Vector2d v, double n)
        {
            return new Vector2d(v.X / n, v.Y / n);
        }

        public static Vector2d operator-(Vector2d v) {
            return new Vector2d(-v.X,-v.Y);
        }

        public override string ToString()
        {
            return StringUtil.FormatReals(X, Y);
        }

        public double AngleTo(Vector2d v) {
            Vector2d vn1 = Normalized;
            Vector2d vn2 = v.Normalized;
            if (vn1 == null || vn2 == null) 
                return 0.0; //any angle will do on zero length vector, we pick 0.0
            return MathUtil.ArcCos(vn1.Dot(vn2));
        }

        public double AngleCCW(Vector2d v)
        {
            double a = AngleTo(v);
            if (PerpDot(v) >= 0.0) return a;
            else return MathUtil.Deg360 - a;
        }

        public double AngleCW(Vector2d v)
        {
            double a = AngleTo(v);
            if (PerpDot(v) <= 0.0) return a;
            else return MathUtil.Deg360 - a;
        }

        public Vector2d Bisect(Vector2d v)
        {
            //use the fact tha if two vectors have the same length, thair bisector is (x1+x2)*0.5 , (y1+y2)*0.5
            //if odd cases, where vector are colinear, pointing in diffrent directions, we just use the perpendicular of this
            double vmag1 = Length;
            double vmag2 = v.Length;

            if (vmag1 == 0.0 || vmag2 == 0.0)
                return null;    //zero length vectors => no bisector

            double x1 = X / vmag1, y1 = Y / vmag1, x2 = v.X / vmag2, y2 = v.Y / vmag2;  //normalize vectors to equal length

            Vector2d res = new Vector2d((x1 + x2) * 0.5, (y1 + y2) * 0.5).Normalized;
            
            if (res == null)
             //this means vectors are colinear, pointing in opposite directions, any of the two perpendicular vectors will do
                return new Vector2d(y1, -x1); //use x1,y1 rotated -90 degrees, thay are already normalized

            return res;
        }


        public double PerpDot(Vector2d v) {return X * v.Y - Y * v.X;}
        public double Dot(Vector2d v) {return X * v.X + Y * v.Y;}
    }
}
