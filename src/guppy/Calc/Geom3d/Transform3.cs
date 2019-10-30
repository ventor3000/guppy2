using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom3d
{
    /*
     * Matrix is of form:
     * 
     * [ax ay az aw]
     * [bx by bz bw]
     * [cx cy cz cw]
     * [tx ty tz tw]
     * where ax,ay,az represent the matrix local xaxis
     * 
     * Matrix can be accessed by this[row,column]
     * */


    /// <summary>
    /// A class for 3d affine transformations.
    /// </summary>
    public class Transform3
    {

        //private double[,] mat = new double[4, 4];

        public readonly double ax, ay, az, aw;
        public readonly double bx, by, bz, bw;
        public readonly double cx, cy, cz, cw;
        public readonly double tx, ty, tz, tw;

        public static readonly Transform3 Identity = new Transform3(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);



        public Transform3(Vector3 xaxis, Vector3 yaxis, Vector3 zaxis)
        {
            ax = xaxis.X;
            ay = xaxis.Y;
            az = xaxis.Z;
            aw = 0.0;

            bx = yaxis.X;
            by = yaxis.Y;
            bz = yaxis.Z;
            bw = 0.0;

            cx = zaxis.X;
            cy = zaxis.Y;
            cz = zaxis.Z;
            cw = 0.0;

            tx = ty = tz = 0.0;
            tw = 1.0;
        }

        public Transform3(
            double ax, double ay, double az, double aw,
            double bx, double by, double bz, double bw,
            double cx, double cy, double cz, double cw,
            double tx, double ty, double tz, double tw)
        {
            this.ax = ax;
            this.ay = ay;
            this.az = az;
            this.aw = aw;

            this.bx = bx;
            this.by = by;
            this.bz = bz;
            this.bw = bw;

            this.cx = cx;
            this.cy = cy;
            this.cz = cz;
            this.cw = cw;

            this.tx = tx;
            this.ty = ty;
            this.tz = tz;
            this.tw = tw;
        }

        /// <summary>
        /// Matrix element access.
        /// </summary>
        /// <param name="row">The row of value (0-3).</param>
        /// <param name="col">The column of the value (0-3)</param>
        /// <returns>The value at the requested position.</returns>
        public double this[int row, int col]
        {
            get
            {
                switch (row)
                {
                    case 0:
                        switch (col)
                        {
                            case 0: return ax;
                            case 1: return ay;
                            case 2: return az;
                            case 3: return aw;
                        }
                        break;
                    case 1:
                        switch (col)
                        {
                            case 0: return bx;
                            case 1: return by;
                            case 2: return bz;
                            case 3: return bw;
                        }
                        break;
                    case 2:
                        switch (col)
                        {
                            case 0: return cx;
                            case 1: return cy;
                            case 2: return cz;
                            case 3: return cw;
                        }
                        break;
                    case 3:
                        switch (col)
                        {
                            case 0: return tx;
                            case 1: return ty;
                            case 2: return tz;
                            case 3: return tw;
                        }
                        break;
                }

                throw new ArgumentException("Invalid index in Transform3 access: " + StringUtil.FormatInt(row) + " , " + StringUtil.FormatReal(col));
            }
        }





        /// <summary>
        /// Multiplies two matrices into a new matrix.
        /// </summary>
        /// <param name="ma">The first matrix to multiply.</param>
        /// <param name="mb">The second matrix to multiply.</param>
        /// <returns>A new matrix.</returns>
        public static Transform3 operator *(Transform3 ma, Transform3 mb)
        {
            //creates a new matrix which is this*mb


            return new Transform3(
                ma.ax * mb.ax + ma.ay * mb.bx + ma.az * mb.cx + ma.aw * mb.tx,
                ma.ax * mb.ay + ma.ay * mb.by + ma.az * mb.cy + ma.aw * mb.ty,
                ma.ax * mb.az + ma.ay * mb.bz + ma.az * mb.cz + ma.aw * mb.tz,
                ma.ax * mb.aw + ma.ay * mb.bw + ma.az * mb.cw + ma.aw * mb.tw,

                ma.bx * mb.ax + ma.by * mb.bx + ma.bz * mb.cx + ma.bw * mb.tx,
                ma.bx * mb.ay + ma.by * mb.by + ma.bz * mb.cy + ma.bw * mb.ty,
                ma.bx * mb.az + ma.by * mb.bz + ma.bz * mb.cz + ma.bw * mb.tz,
                ma.bx * mb.aw + ma.by * mb.bw + ma.bz * mb.cw + ma.bw * mb.tw,

                ma.cx * mb.ax + ma.cy * mb.bx + ma.cz * mb.cx + ma.cw * mb.tx,
                ma.cx * mb.ay + ma.cy * mb.by + ma.cz * mb.cy + ma.cw * mb.ty,
                ma.cx * mb.az + ma.cy * mb.bz + ma.cz * mb.cz + ma.cw * mb.tz,
                ma.cx * mb.aw + ma.cy * mb.bw + ma.cz * mb.cw + ma.cw * mb.tw,

                ma.tx * mb.ax + ma.ty * mb.bx + ma.tz * mb.cx + ma.tw * mb.tx,
                ma.tx * mb.ay + ma.ty * mb.by + ma.tz * mb.cy + ma.tw * mb.ty,
                ma.tx * mb.az + ma.ty * mb.bz + ma.tz * mb.cz + ma.tw * mb.tz,
                ma.tx * mb.aw + ma.ty * mb.bw + ma.tz * mb.cw + ma.tw * mb.tw
            );


        }

        /// <summary>
        /// Creates a new translation matrix using a specified dx,dy,dz.
        /// </summary>
        /// <param name="dx">Delta x for translation.</param>
        /// <param name="dy">Delta y for translation.</param>
        /// <param name="dz">Delta z for translation.</param>
        /// <returns>A new translation matrix.</returns>
        public static Transform3 Translate(double dx, double dy, double dz)
        {
            return new Transform3(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                dx, dy, dz, 1.0
            );
        }


        public static Transform3 Scale(double s)
        {
            return new Transform3(
                s, 0, 0, 0,
                0, s, 0, 0,
                0, 0, s, 0,
                0, 0, 0, 1
                );
        }

        public static Transform3 Stretch(double sx, double sy, double sz)
        {
            return new Transform3(
                sx, 0, 0, 0,
                0, sy, 0, 0,
                0, 0, sz, 0,
                0, 0, 0, 1
                );
        }


        /// <summary>
        /// Creates a rotation matrix around a ray given by a start point and a vector.
        /// </summary>
        /// <param name="origin">The start point of the ray to rotate about.</param>
        /// <param name="axisin">The extension of the ray to rotate about</param>
        /// <param name="angle">The angle in radians to rotate with.</param>
        /// <returns>A new rotation matrix.</returns>
        public static Transform3 Rotate(Point3 rayorigin, Vector3 raydirection, double radians)
        {
            //rotate about an arbitrary axis
            //defined by an origin and an orientation vector.

            Vector3 axis = raydirection.Normalized;

            double c = Math.Cos(radians);
            double s = Math.Sin(radians);
            double t = 1.0 - c;
            double tx = t * axis.X;
            double ty = t * axis.Y;
            double tz = t * axis.Z;
            double sx = s * axis.X;
            double sy = s * axis.Y;
            double sz = s * axis.Z;
            double dx = rayorigin.X;
            double dy = rayorigin.Y;
            double dz = rayorigin.Z;


            double x0 = tx * axis.X + c;
            double y0 = tx * axis.Y + sz;
            double z0 = tx * axis.Z - sy;
            double x1 = ty * axis.X - sz;
            double y1 = ty * axis.Y + c;
            double z1 = ty * axis.Z + sx;
            double x2 = tz * axis.X + sy;
            double y2 = tz * axis.Y - sx;
            double z2 = tz * axis.Z + c;

            return new Transform3(
                x0, y0, z0, 0.0,
                x1, y1, z1, 0.0,
                x2, y2, z2, 0.0,
                dx - dx * x0 - dy * x1 - dz * x2,
                dy - dx * y0 - dy * y1 - dz * y2,
                dz - dx * z0 - dy * z1 - dz * z2,
                1.0);

        }


        public Vector3 XAxis
        {
            get
            {
                return new Vector3(ax, ay, az);
            }
        }

        public Vector3 YAxis
        {
            get
            {
                return new Vector3(bx, by, bz);
            }
        }

        public Vector3 ZAxis
        {
            get
            {
                return new Vector3(cx, cy, cz);
            }
        }

        public Vector3 TransformVector
        {
            get
            {
                return new Vector3(tx, ty, tz);
            }
        }

        public Transform3 Inverse
        {
            get
            {
                double m3344S3443 = cz * tw - cw * tz;
                double m3244S3442 = cy * tw - cw * ty;
                double m3243S3342 = cy * tz - cz * ty;

                double m3144S3441 = cx * tw - cw * tx;
                double m3143S3341 = cx * tz - cz * tx;

                double m3142S3241 = cx * ty - cy * tx;

                double m2344S2443 = bz * tw - bw * tz;
                double m2244S2442 = by * tw - bw * ty;
                double m2243S2342 = by * tz - bz * ty;

                double m2144S2441 = bx * tw - bw * tx;
                double m2143S2341 = bx * tz - bz * tx;

                double m2142S2241 = bx * ty - by * tx;

                double m2334S2433 = bz * cw - bw * cz;
                double m2234S2432 = by * cw - bw * cy;
                double m2233S2332 = by * cz - bz * cy;

                double m2134S2431 = bx * cw - bw * cx;
                double m2133S2331 = bx * cz - bz * cx;

                double m2132S2231 = bx * cy - by * cx;

                double A11 = by * m3344S3443 - bz * m3244S3442 + bw * m3243S3342;
                double A12 = -(bx * m3344S3443 - bz * m3144S3441 + bw * m3143S3341);
                double A13 = bx * m3244S3442 - by * m3144S3441 + bw * m3142S3241;
                double A14 = -(bx * m3243S3342 - by * m3143S3341 + bz * m3142S3241);

                double A21 = -(ay * m3344S3443 - az * m3244S3442 + aw * m3243S3342);
                double A22 = ax * m3344S3443 - az * m3144S3441 + aw * m3143S3341;
                double A23 = -(ax * m3244S3442 - ay * m3144S3441 + aw * m3142S3241);
                double A24 = ax * m3243S3342 - ay * m3143S3341 + az * m3142S3241;

                double A31 = ay * m2344S2443 - az * m2244S2442 + aw * m2243S2342;
                double A32 = -(ax * m2344S2443 - az * m2144S2441 + aw * m2143S2341);
                double A33 = ax * m2244S2442 - ay * m2144S2441 + aw * m2142S2241;
                double A34 = -(ax * m2243S2342 - ay * m2143S2341 + az * m2142S2241);

                double A41 = -(ay * m2334S2433 - az * m2234S2432 + aw * m2233S2332);
                double A42 = ax * m2334S2433 - az * m2134S2431 + aw * m2133S2331;
                double A43 = -(ax * m2234S2432 - ay * m2134S2431 + aw * m2132S2231);
                double A44 = ax * m2233S2332 - ay * m2133S2331 + az * m2132S2231;

                //Calc out the determinant.
                double detA = ax * A11;

                double detB = ay * A12;

                double detC = az * A13;

                double detD = aw * A14;

                double det = (detA + detB + detC + detD);

                double invdet = 0.0f;
                if (Math.Abs(det) < 1e-50)
                    throw new Exception("Inversion of this matrix not possible");
                else
                    invdet = 1.0f / det;



                return new Transform3(
                   A11 * invdet,
                   A21 * invdet,
                   A31 * invdet,
                   A41 * invdet,

                   A12 * invdet,
                   A22 * invdet,
                   A32 * invdet,
                   A42 * invdet,


                   A13 * invdet,
                   A23 * invdet,
                   A33 * invdet,
                   A43 * invdet,

                   A14 * invdet,
                   A24 * invdet,
                   A34 * invdet,
                   A44 * invdet
               );


            }
        }


        public void Apply(double x, double y, double z, out double transx, out double transy, out double transz, bool translate)
        {
            if (translate)
            {
                double transw =
                   x * aw +
                   y * bw +
                   z * cw +
                   tw;
                transx =
                  (x * ax +
                   y * bx +
                   z * cx +
                   tx) / transw;
                transy =
                  (x * ay +
                   y * by +
                   z * cy +
                   ty) / transw;
                transz =
                  (x * az +
                   y * bz +
                   z * cz +
                   tz) / transw;
            }
            else
            {
                double vx = x, vy = y, vz = z;
                transx = x * ax + vy * bx + vz * cx;
                transy = x * ay + vy * by + vz * cy;
                transz = x * az + vy * bz + vz * cz;
            }
        }

        public override string ToString()
        {
            return string.Join(" , ", 
                StringUtil.FormatReal(ax), StringUtil.FormatReal(ay), StringUtil.FormatReal(az), StringUtil.FormatReal(aw),
                StringUtil.FormatReal(bx), StringUtil.FormatReal(by), StringUtil.FormatReal(bz), StringUtil.FormatReal(bw),
                StringUtil.FormatReal(cx), StringUtil.FormatReal(cy), StringUtil.FormatReal(cz), StringUtil.FormatReal(cw),
                StringUtil.FormatReal(tx), StringUtil.FormatReal(ty), StringUtil.FormatReal(tz), StringUtil.FormatReal(tw)
            );

            
        }
    }

}
