using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.Calc.Geom2d
{
    public class Transform2d
    {
        public readonly double AX, AY, BX, BY, TX, TY;

        public static readonly Transform2d Identity = new IdentityTransform2d();

        public Transform2d(double ax, double ay, double bx, double by, double tx, double ty)
        {
            this.AX = ax;
            this.AY = ay;
            this.BX = bx;
            this.BY = by;
            this.TX = tx;
            this.TY = ty;
        }

        public bool IsIdentity
        {
            get
            {
                if (this == Identity) return true;
                return (AX == 1.0 && AY == 0.0 && BX == 0.0 && BY == 1.0 && TX == 0.0 && TY == 0.0);
            }
        }
        
        public static Transform2d operator *(Transform2d a, Transform2d b)
        {
            return new Transform2d(
                a.AX * b.AX + a.AY * b.BX,
                a.AX * b.AY + a.AY * b.BY,
                a.BX * b.AX + a.BY * b.BX,
                a.BX * b.AY + a.BY * b.BY,
                a.TX * b.AX + a.TY * b.BX + b.TX, 
                a.TX * b.AY + a.TY * b.BY + b.TY 
            );
        }

        public Matrix ToMatrix()
        {
            /*return new Matrix(3, 3,
                AX, AY, 0.0,
                BX, BY, 0.0,
                TX, TY, 1.0);*/

            return new Matrix(3, 3,
                AX, BX, TX,
                AY, BY, TY,
                0, 0, 1
                );
        }

        public static Transform2d Stretch(double sx, double sy)
        {
            return new Transform2d(sx, 0, 0, sy, 0, 0);
        }

        public static Transform2d Scale(double s)
        {
            return Stretch(s, s);
        }


        public static Transform2d Rotate(double rad)
        {
            double s = Math.Sin(rad);
            double c = Math.Cos(rad);
            return new Transform2d(c, s, -s, c, 0, 0);
        }

        public static Transform2d Translate(double dx, double dy)
        {
            return new Transform2d(1, 0, 0, 1, dx, dy);
        }

        public static Transform2d Translate(Vector2d v)
        {
            return new Transform2d(1, 0, 0, 1, v.X, v.Y);
        }


        public static Transform2d Skew(double delta_xangle, double delta_yangle)
        {
            //TODO: make smarter or at least test if this works at all
            return new Transform2d(Math.Cos(delta_xangle), Math.Sin(delta_xangle), Math.Cos(delta_yangle + MathUtil.Deg90), Math.Sin(delta_yangle + MathUtil.Deg90),0.0,0.0);
        }


        public virtual void Apply(double x, double y, out double xp, out double yp, bool translate)
        {

            //| AX BX TX |   | X |
            //| AY BY TY | * | Y |
            //| 0  0  1  |   | 1 |

            
            if (translate)
            {
                xp = x * AX + y * BX + TX;
                yp = x * AY + y * BY + TY;
            }
            else
            {
                xp = x * AX + y * BX;
                yp = x * AY + y * BY;
            }
        }

        public double Determinant
        {
            get
            {
                return AX * BY - BX * AY;
            }
        }

        public Transform2d Inversed
        {
            get
            {
                double d = Determinant;
                if (d==0.0)
                    throw new Exception("Zero determinant matrix cannot be inverted");

                return new Transform2d(
                    BY / d, -AY / d,
                    -BX / d, AX / d,
                    (BX * TY - BY * TX) / d,
                    (AY * TX - AX * TY) / d
                    );
            }
        }


        public override string ToString()
        {
            return StringUtil.FormatReals(AX, AY, BX, BY, TX, TY);
        }

        /// <summary>
        /// Check if the transform is uniform, which means (for example) it cannot change a circle into an ellipse.
        /// </summary>
        public bool IsUniform
        {
            get
            {
                //Dot product to check if axes are perpencdicular
                if (MathUtil.IsZero(AX * BX + AY * BY))
                {
                    //same length of axes
                    if (Math.Abs((AX * AX + AY * AY) - (BX * BX + BY * BY)) < MathUtil.Epsilon)
                        return true;
                }

                return false;
            }
        }

        private class IdentityTransform2d : Transform2d
        {

            public IdentityTransform2d() : base(1.0, 0.0, 0.0, 1.0, 0.0, 0.0) { }

            public override void Apply(double x, double y, out double xp, out double yp, bool translate)
            {
                xp = x;
                yp = y;
            }
        }
       
    }
}
