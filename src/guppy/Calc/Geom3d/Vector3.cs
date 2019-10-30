using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom3d
{
    public class Vector3
    {

        public static readonly Vector3 XAxis=new Vector3(1,0,0);
        public static readonly Vector3 YAxis=new Vector3(0,1,0);
        public static readonly Vector3 ZAxis=new Vector3(0,0,1);
        
        public readonly double X, Y, Z;

        /// <summary>
        /// Creates a vector of zero length.
        /// </summary>
        public Vector3()
        {
            X = Y = Z = 0.0;
        }

        

        /// <summary>
        /// Creates a vector with specified x,y,z components.
        /// </summary>
        /// <param name="_x">The x component for the vector.</param>
        /// <param name="_y">The y component for the vector.</param>
        /// <param name="_z">The z component for the vector.</param>
        public Vector3(double _x, double _y, double _z)
        {
            X = _x;
            Y = _y;
            Z = _z;
        }



        /// <summary>
        /// Modifies the vector in-place to have unit length.
        /// </summary>
        public Vector3 Normalized
        {
            get
            {
                double l = Magnitude;
                if (MathUtil.IsZero(l))
                    //normalizing zero length vector?
                    throw new Exception("Normalize called for zero length Vector3d");

                double invlen = 1.0 / l;
                return new Vector3(X * invlen, Y * invlen, Z * invlen);
            }
        }

        

        public bool IsDirectionalWith(Vector3 vec)
        {
            Vector3 v1 = Normalized;
            Vector3 v2 = vec.Normalized;
            return v1.Equals(v2, MathUtil.Epsilon);
        }

        /// <summary>
        /// Computes the length of the vector.
        /// </summary>
        /// <returns>The length of the vector.</returns>
        public double Magnitude
        {
            get { return Math.Sqrt(X * X + Y * Y + Z * Z); }
        }

        /// <summary>
        /// Computes the squared length of the vector, which is faster than computing its length.
        /// </summary>
        /// <returns>The vectors squared length.</returns>
        public double SquaredMagnitude
        {
            get
            {
                return X * X + Y * Y + Z * Z;
            }
        }

        /// <summary>
        /// Computes the dot product between this vector and another.
        /// If the vectors are normalized this is the same as cosine of the smallest angle between vectors.
        /// </summary>
        /// <param name="v">The other vector.</param>
        /// <returns>A dot product.</returns>
        public double Dot(Vector3 v)
        {
            return X * v.X + Y * v.Y + Z * v.Z;
        }

        public Vector3 Cross(Vector3 v)
        {
            return new Vector3(Y * v.Z - Z * v.Y, Z * v.X - X * v.Z, X * v.Y - Y * v.X);
        }

        public double this[int idx]
        {
            get
            {
                int i = idx % 3;
                if (i == 0)
                    return X;
                else if (i == 1)
                    return Y;
                return Z;
            }
        }
        
        /// <summary>
        /// Multiplies the length of the vector with a factor (scales it)
        /// </summary>
        /// <param name="scale">The factor to multiply with</param>
        /// <param name="v">The vector to scale.</param>
        /// <returns>A new vector which has the same direction as the one sent but length multiplied with the factor.</returns>
        public static Vector3 operator *(double scale, Vector3 v)
        {
            return new Vector3(v.X * scale, v.Y * scale, v.Z * scale);
        }

        /// <summary>
        /// Multiplies the magnitude of the vector with a factor (scales it)
        /// </summary>
        /// <param name="v">The vector to scale.</param>
        /// <param name="scale">The factor to multiply with</param>
        /// <returns>A new vector which has the same direction as the one sent but length multiplied with the factor.</returns>
        public static Vector3 operator *(Vector3 v, double scale)
        {
            return scale * v;
        }

        public static Vector3 operator *(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }

        public static Vector3 operator +(double scale, Vector3 v)
        {
            return new Vector3(v.X + scale, v.Y + scale, v.Z + scale);
        }

        public static Vector3 operator +(Vector3 v, double scale)
        {
            return scale + v;
        }
        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector3 operator -(double scale, Vector3 v)
        {
            return new Vector3(v.X - scale, v.Y - scale, v.Z - scale);
        }
        public static Vector3 operator -(Vector3 v, double scale)
        {
            return scale - v;
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.X, -v.Y, -v.Z);
        }

        public static Vector3 operator /(double scale, Vector3 v)
        {
            return new Vector3(v.X / scale, v.Y / scale, v.Z / scale);
        }

        public static Vector3 operator /(Vector3 v, double scale)
        {
            return scale / v;
        }

        public static Vector3 operator /(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
        }

        public static bool operator ==(Vector3 v1, Vector3 v2)
        {
            return Equals(v1, v2);
        }

        public static bool operator !=(Vector3 v1, Vector3 v2)
        {
            return !Equals(v1, v2);
        }

        public static bool operator <(Vector3 v1, Vector3 v2)
        {
            return v1.SquaredMagnitude < v2.SquaredMagnitude;
        }

        public static bool operator >(Vector3 v1, Vector3 v2)
        {
            return v1.SquaredMagnitude > v2.SquaredMagnitude;
        }

        public bool Equals(Vector3 v, double tol)
        {
            double dx = X - v.X;
            double dy = Y - v.Y;
            double dz = Z - v.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz) <= tol;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3)
                return (Equals((Vector3)obj, MathUtil.Epsilon));
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode();
        }

        /// <summary>
        /// Creates a string representation of the vector suitable for debugging.
        /// </summary>
        /// <returns>A string with vector data.</returns>
        public override string ToString()
        {
            return StringUtil.FormatReal(X) + " , " + StringUtil.FormatReal(Y) + ", " + StringUtil.FormatReal(Z);
        }

        /*public Vector3 Transform(Matrix3d m)
        {
            double vx = X, vy = Y, vz = Z;

            return new Vector3(vx * m[0, 0] + vy * m[1, 0] + vz * m[2, 0],
                Y = vx * m[0, 1] + vy * m[1, 1] + vz * m[2, 1],
                Z = vx * m[0, 2] + vy * m[1, 2] + vz * m[2, 2]);
        }*/

        public Point3 ToPoint()
        {
            return new Point3(X, Y, Z);
        }

        /// <summary>
        /// Computes the inner angle between this vector and another
        /// </summary>
        public double Angle(Vector3 vec)
        {
            double cs, len;
            if ((len = Magnitude * vec.Magnitude) == 0.0)
                return 0.0;
            cs = Dot(vec) / len;
            return Math.Acos(MathUtil.Clamp(cs, -1.0, 1.0));
        }

        public Vector3 Transform(Transform3 t)
        {
            double tx,ty,tz;
            t.Apply(X, Y, Z, out tx, out ty, out tz, false);
            return new Vector3(tx, ty, tz);
        }

    }
}
