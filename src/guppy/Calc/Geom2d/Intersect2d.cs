using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{
    public static class Intersect2d
    {

        const double seg_minparam = 0.0;    //used as min limit for parameters beeing on a segment
        const double seg_maxparam = 1.0;    //used as max limit for parameters beeing on a segment

        #region PRIVATE_UTILS

        /// <summary>
        /// Converts an array of line parameters to an array of points, limited
        /// by a minimum and maximum parameter. On emty result, null is returned,
        /// otherwise a list of points along the line.
        /// </summary>
        private static Point2d[] LineParamsToPoints(double[] ts, Line2d l, double mint, double maxt)
        {
            if (ts == null) return null;
            int n = ts.Length;
            if (n <= 0)
                return null;

            Point2d[] res = new Point2d[n];
            int idx = 0;
            for (int q = 0; q < n; q++)
            {
                double t = ts[q];
                if (t >= mint && t <= maxt)
                    res[idx++] = l.PointAt(ts[q]);
            }

            if (idx == 0)
                return null;    //all params out of range

            if (idx != n)
                Array.Resize(ref res, idx);

            return res;
        }

        private static double[] GetRealRoots(double c4,double c3,double c2,double c1,double c0)
        {
            RealPolynomial rp = new RealPolynomial(c4, c3, c2, c1, c0);
            return rp.FindRoots(true);
        }

        private static double[] GetRealRoots(double c3, double c2, double c1, double c0)
        {
            RealPolynomial rp = new RealPolynomial(c3, c2, c1, c0);
            return rp.FindRoots(true);
        }


        #endregion

        #region CIRCLE_DOMINANT

        public static Point2d[] CircleCircle(Circle2d c1, Circle2d c2)
        {
            double d = c1.Center.Distance(c2.Center);

            if (d > c1.Radius + c2.Radius || d < Math.Abs(c1.Radius - c2.Radius))
                return null; //totally inside or totally outside

            //distance to radical line:
            double radd = (d * d - c2.Radius * c2.Radius + c1.Radius * c1.Radius) / (2.0 * d);

            //midpoint of radical line in radx,rady
            double dx = c2.Center.X - c1.Center.X;
            double dy = c2.Center.Y - c1.Center.Y;
            double radx = c1.Center.X + (dx / d) * radd;
            double rady = c1.Center.Y + (dy / d) * radd;

            //intersect circle with radical line
            return CircleSeg(c1, new Line2d(radx, rady, radx + dy, rady - dx));
        }

        public static double[] CircleLineParametric(Circle2d c, Line2d l)
        {
            double dx = l.X2 - l.X1;
            double dy = l.Y2 - l.Y1;
            double px = l.X1;
            double py = l.Y1;
            double cx = c.Center.X;
            double cy = c.Center.Y;

            double pa = dx * dx + dy * dy;
            double pb = 2.0 * (px * dx - cx * dx + py * dy - cy * dy);
            double pc = px * px - 2.0 * px * cx + cx * cx +
                        py * py - 2.0 * py * cy + cy * cy - c.Radius * c.Radius;

            return RealPolynomial.SolveQuadric(pa, pb, pc);
        }

        public static Point2d[] CircleSeg(Circle2d c, Line2d l)
        {
            return LineParamsToPoints(CircleLineParametric(c, l), l,seg_minparam,seg_maxparam);
        }

        public static Point2d[] CircleLine(Circle2d c, Line2d l)
        {
            return LineParamsToPoints(CircleLineParametric(c, l), l, double.NegativeInfinity,double.PositiveInfinity);
        }

        public static Point2d[] CircleRay(Circle2d c, Line2d l)
        {
            return LineParamsToPoints(CircleLineParametric(c, l), l, seg_minparam, double.PositiveInfinity);
        }
        
        #endregion

        #region ELLIPSE_DOMINANT

        public static Point2d[] EllipseEllipse(Ellipse2d el1, Ellipse2d el2)
        {
            //reduce this problem to a circle-ellipse problem by
            //rotating ellipse 1 down, scaling it to circle and then
            //rotate ellipse2 down.
            Transform2d tr = Transform2d.Rotate(-el1.Rotation) * Transform2d.Stretch(1.0, 1.0 / el1.Ratio);

            //dont modify originals:
            el1 = new Ellipse2d(el1);
            el2 = new Ellipse2d(el2);
            el1.Transform(tr);
            el2.Transform(tr);
            
            Point2d[] res = EllipseCircle(el2, new Circle2d(el1.X, el1.Y, el1.MajorRadius));

            if (res == null)
                return null;

            Transform2d trinv = (tr).Inversed;
            for (int l = 0; l < res.Length; l++)
                res[l] = res[l].GetTransformed(trinv);

            return res;
        }

        public static Point2d[] EllipseEllipse2(Ellipse2d elp1, Ellipse2d elp2)
        {
            //TODO: check if this is better than EllipseEllipse in stabillity and replace it or remove this function

            Transform2d tr = elp1.ToStandardPosition;

            elp2 = new Ellipse2d(elp2); //dont alter the original ellipse
            elp2.Transform(tr);

            elp1 = new Ellipse2d(elp1);
            elp1.Transform(tr);

            GeneralConic2d con1 = new GeneralConic2d(1.0, 0.0, 1 / (elp1.Ratio * elp1.Ratio), 0.0, 0.0, -1.0);
            GeneralConic2d con2 = elp2.ToGeneralConic(); // GeneralConic2d.FromEllipse(elp2);

            Point2dSet pset = new Point2dSet();
            pset.AddRange(ConicConic(con1, con2));
            pset.Transform(tr.Inversed);
            return pset.ToArray();
        }

        public static Point2d[] EllipseCircle(Ellipse2d el, Circle2d ci)
        {
            Transform2d tr = el.ToStandardPosition;
            ci = new Circle2d(ci); //dont modify original circle, but this copy
            ci.Transform(tr);

            double b = el.Ratio, b2 = b * b, b4 = b2 * b2;
            double i = ci.Center.X, i2 = i * i, i4 = i2 * i2;
            double j = ci.Center.Y, j2 = j * j, j4 = j2 * j2;
            double r = ci.Radius, r2 = r * r, r4 = r2 * r2;

            double x4 = b4 - 2 * b2 + 1;
            double x3 = 4 * b2 * i - 4 * i;
            double x2 = b2 * (2 * r2 + 2 * j2 - 2 * i2 + 2) - 2 * r2 + 2 * j2 + 6 * i2 - 2 * b4;
            double x1 = 4 * i * r2 - 4 * i * j2 - 4 * i * i * i - 4 * b2 * i;
            double x0 = r4 + (-2 * j2 - 2 * i2) * r2 + b2 * (-2 * r2 - 2 * j2 + 2 * i2) + j4 + 2 * i2 * j2 + i4 + b4;
            //double[] xs = RealPolynomial.SolveQuartic2(x4, x3, x2, x1, x0, 1e-30);

            RealPolynomial rp = new RealPolynomial(x4, x3, x2, x1, x0);
            double[] xs = rp.FindRoots(true);

            if (xs == null) return null;    //no intersections


            Point2dSet resultset = new Point2dSet();

            foreach (double x in xs)
            {
                //test the two possible y:s to be solutions for this x
                double y = (1 - x * x) * b2;
                if (y < 0.0) continue;
                y = Math.Sqrt(y);

                for (int t = 0; t < 2; t++) //test booth y solutions...
                {
                    double err = x * x + y * y / b2 - 1.0; //on ellipse
                    double err2 = MathUtil.Square(x - i) + MathUtil.Square(y - j) - r2; //on circle
                    if (MathUtil.IsZero(err, 1e-7) && MathUtil.IsZero(err2, MathUtil.Epsilon)) 
                        resultset.Add(new Point2d(x, y));

                    y = -y;  // ...by inverting y in second turn
                }
            }

            if (resultset.Count == 0) return null;

            resultset.Transform(tr.Inversed); //back to original position

            return resultset.ToArray();
        }


        #endregion
        
        #region LINE_DOMINANT

        static public bool LineLineParametric(Line2d l1, Line2d l2, out double line1_param, out double line2_param)
        {
            double denom = l1.X1 * (l2.Y2 - l2.Y1) + l1.X2 * (l2.Y1 - l2.Y2) + l2.X2 * (l1.Y2 - l1.Y1) + l2.X1 * (l1.Y1 - l1.Y2);
            if (Math.Abs(denom) > 0.0)
            { //not parallel
                //s is intersection parameter on l1
                double s = (l1.X1 * (l2.Y2 - l2.Y1) + l2.X1 * (l1.Y1 - l2.Y2) + l2.X2 * (l2.Y1 - l1.Y1)) / denom;
                //t is intersection parameter on l2
                double t = -(l1.X1 * (l2.Y1 - l1.Y2) + l1.X2 * (l1.Y1 - l2.Y1) + l2.X1 * (l1.Y2 - l1.Y1)) / denom;
                line1_param = s;
                line2_param = t;
                return true;
            }
            line1_param = 0.0;
            line2_param = 0.0;
            return false;
        }

        static public Point2d[] LineLine(Line2d lin1, Line2d lin2)
        {
            double s, t;
            if (!LineLineParametric(lin1, lin2, out s, out t)) return null;
            return new Point2d[] { lin1.PointAt(s) };
        }

        static public Point2d LineLineAsPoint(Line2d lin1, Line2d lin2)
        {
            double s, t;
            if (!LineLineParametric(lin1, lin2, out s, out t)) 
                return null;
            return lin1.PointAt(s);
        }

        #endregion

        #region HYPERBOLA_DOMINANT

        public static double[] HyperbolaLineParametric(Hyperbola2d hyp,Line2d lin) {
            // Computes intersection points with a hyperbola and a line
            // solved and created by Robert.P. 2013-02-11
            //
            // solution is created by transforming system to hyperbola standard position,
            // and then solving the system x^2/a^2-y^2/b^2-1=0 , x=x0+t*dx and y=y0+t*dy (note that a is 1 in std. pos.)
            
            Transform2d tr = hyp.ToStandardPosition;

            //extract standard spaced line
            double x0, y0, x1, y1, dx, dy, b = hyp.Ratio;
            tr.Apply(lin.X1, lin.Y1, out x0, out y0, true);
            tr.Apply(lin.X2, lin.Y2, out x1, out y1, true);
            dx = x1 - x0; dy = y1 - y0;

            double t2 = -dy * dy + b * b * dx * dx;
            double t1 = -2 * dy * y0 + 2 * b * b * dx * x0;
            double t0 = -y0 * y0 + b * b * x0 * x0 - b * b;

            return RealPolynomial.SolveQuadric(t2, t1, t0, 0.0);
        }


        public static Point2d[] HyperbolaSeg(Hyperbola2d hyp, Line2d seg)
        {
            return LineParamsToPoints(HyperbolaLineParametric(hyp, seg),seg,seg_minparam,seg_maxparam);
        }

        public static Point2d[] HyperbolaLine(Hyperbola2d hyp, Line2d lin)
        {
            return LineParamsToPoints(HyperbolaLineParametric(hyp, lin), lin, double.NegativeInfinity, double.PositiveInfinity);
        }

        public static Point2d[] HyperbolaRay(Hyperbola2d hyp, Line2d ray)
        {
            return LineParamsToPoints(HyperbolaLineParametric(hyp, ray), ray, seg_minparam, double.PositiveInfinity);
        }

        public static Point2d[] HyperbolaEllipse(Hyperbola2d hyp, Ellipse2d elp)
        {
            //TODO: this is probably more stable intersecting hyperbola with unitcircle. Rewrite.

            Transform2d tr = hyp.ToStandardPosition;
            hyp = new Hyperbola2d(hyp);
            elp = new Ellipse2d(elp);
            hyp.Transform(tr);
            elp.Transform(tr);

            GeneralConic2d hcon = new GeneralConic2d(1, 0.0, -1 / (hyp.B * hyp.B), 0.0, 0.0, -1);

            Point2dSet pset = new Point2dSet();
            pset.AddRange(ConicConic(hcon, elp.ToGeneralConic()));
            pset.Transform(tr.Inversed);
            return pset.ToArray();
        }


        /// <summary>
        /// Intersect a hyperbola in standard position (a=1.0) with a general conic.
        /// Helper function for other hyperbola intersection functions.
        /// </summary>
        /// <param name="hyp1_b"></param>
        /// <param name="hyp2"></param>
        private static Point2dSet StdHyperbolaConic(double b1,Conic2d con) {



            double R = 1.0 / (b1 * b1);

            Point2dSet res=new Point2dSet();
            GeneralConic2d gc=con.ToGeneralConic();
            double A=gc.A,B=gc.B,C=gc.C,D=gc.D,E=gc.E,F=gc.F;

            double y4=A*A*R*R+(2*A*C-B*B)*R+C*C;
            double y3=(2*A*E-2*B*D)*R+2*C*E;
            double y2=(-D*D+2*A*A+2*F*A)*R+E*E+(2*A+2*F)*C-B*B;
            double y1=(2*A+2*F)*E-2*B*D;
            double y0=A*A+2*F*A+F*F-D*D;

            double[] ys=GetRealRoots(y4,y3,y2,y1,y0);
            if(ys==null)
                return null;

            foreach(double y in ys) {
                //two possible x for each y. Try to find the correct one:
                double x=Math.Sqrt(y*y*R+1);
                double nx=-x;
                double err=A*x*x+B*x*y+C*y*y+D*x+E*y+F;
                double nerr=A*nx*nx+B*nx*y+C*y*y+D*nx+E*y+F;

                if(Math.Abs(err)<Math.Abs(nerr))
                    res.Add(new Point2d(x,y));
                else
                    res.Add(new Point2d(nx,y));
            }

            return res;
        }

        public static Point2d[] HyperbolaHyperbola(Hyperbola2d hyp1,Hyperbola2d hyp2) {
            Transform2d tr=hyp1.ToStandardPosition;
            hyp2 = new Hyperbola2d(hyp2); //copy for modification
            
            hyp2.Transform(tr);
            Point2dSet res=StdHyperbolaConic(hyp1.Ratio, hyp2);
            res.InverseTransform(tr);
            return res.ToArray();
        }

        #endregion

        #region CONIC_DOMINANT

        public static Point2d[] ConicConic(GeneralConic2d tcon1, GeneralConic2d tcon2)
        {

            // uses the beautiful solution to find the multiplier λ, so that Conic11+λ*Conic2 is
            // a degenerate conic, that is a conic of lines (the 'pencil'). The lines in this conic
            // intersects each of the conics in their common intersection points, thus the problem
            // has been reduced to a Conic-Line intersection problem.
            // This technique is described in book Graphics Gems V, of which we are inspired although
            // this code differs a somewhat from the one in the book.

            // work in standard space for conic 1, gives a more stable computation and speeds up
            // the multiple line-conic intersections later on.
            GeneralConic2d con1 = new GeneralConic2d(tcon1);
            GeneralConic2d con2 = new GeneralConic2d(tcon2);
           
            

            //TODO: does not work properly, probably because line extractor does not work correctly in some cases


            //convert conic coefficients to their matrix form
            double a = con1.A, b = con1.B * 0.5, c = con1.C, d = con1.D * 0.5, e = con1.E * 0.5, f = con1.F;
            double A = con2.A, B = con2.B * 0.5, C = con2.C, D = con2.D * 0.5, E = con2.E * 0.5, F = con2.F;


            //TODO: since conic 1 is in standard position, thoose can be simplified: b,d,e terms are always zero
            double c3 = (A * C - B * B) * F - A * E * E + 2 * B * D * E - C * D * D;
            double c2 = (a * C - 2 * b * B + c * A) * F - a * E * E + (2 * b * D + 2 * d * B - 2 * e * A) * E - c * D * D + (2 * e * B - 2 * d * C) * D + f * A * C - f * B * B;
            double c1 = (a * c - b * b) * F + (2 * b * d - 2 * a * e) * E + (2 * b * e - 2 * c * d) * D + (a * f - d * d) * C + (2 * d * e - 2 * b * f) * B + (c * f - e * e) * A;
            double c0 = (a * c - b * b) * f - a * e * e + 2 * b * d * e - c * d * d;

            double[] lambdas2 = RealPolynomial.SolveCubic2(c3, c2, c1, c0); //up to three coefficients that will turn conic sum to degenerate lines
            double[] lambdas = GetRealRoots(c3, c2, c1, c0);

            
            
            if (lambdas == null) 
                return null; //this can never happen on a 3d degree equation but we check it anyway

            Point2dSet res = new Point2dSet();
            foreach (double lambda in lambdas)
            {
                GeneralConic2d pencil = new GeneralConic2d(
                    a + lambda * A,
                    (b + lambda * B) * 2,
                    c + lambda * C,
                    (d + lambda * D) * 2,
                    (e + lambda * E) * 2,
                    f + lambda * F);

                Line2d[] lines=pencil.ToLines();

                if (lines != null)
                {
                    foreach (Line2d lin in lines)
                    { //max 2 lines
                        Point2d[] intpts = ConicLine(con1, lin);
                        if (intpts == null) continue;

                        //validate each point satisfying the conic equations (they can be out of range for finite conics such as ellipses)
                        foreach (Point2d pt in intpts)
                        {
                            double x = pt.X;
                            double y = pt.Y;
                            double err1 = con1.A * x * x + con1.B * x * y + con1.C * y * y + con1.D * x + con1.E * y + con1.F;
                            if (MathUtil.IsZero(err1,5.0))
                            {
                                double err2 = con2.A * x * x + con2.B * x * y + con2.C * y * y + con2.D * x + con2.E * y + con2.F;
                                if (MathUtil.IsZero(err2,5.0))
                                {
                                    res.Add(pt);
                                }
                            }
                        }
                    }
                }
            }

            //res.Transform(tr.Inversed);
            return res.ToArray();
        }


        /// <summary>
        /// Intersects a line with a conic that is in standard position, that is, not rotated and
        /// centered at 0,0
        /// </summary>
        /// <param name="con"></param>
        /// <param name="lin"></param>
        /// <returns>A list of parameters on line that is intersection points, or null if no intersections</returns>
        private static double[] ConicLineParametric(GeneralConic2d con, Line2d lin)
        {

            //We construct a matrix so that: conic is unrotated (B term=0) and line starts at origo and has length=1.0
            //This is to improve stabillity of the equation

            double invlen = 1.0 / lin.Length;
            if (double.IsInfinity(invlen))
                return null;    //zero length line does not intersect

            Transform2d tr = Transform2d.Translate(-lin.X1, -lin.Y1) * Transform2d.Rotate(-con.Rotation) * Transform2d.Scale(invlen);

            GeneralConic2d c = new GeneralConic2d(con);  //copy for modification
            double x1 = lin.X2, y1 = lin.Y2;
            c.Transform(tr);
            tr.Apply(x1, y1, out x1, out y1, true); //transformed line end
            

            double t2=y1*y1*c.C+x1*x1*c.A;
            double t1=y1*c.E+x1*c.D;
            double t0=c.F;

            double[] ts=RealPolynomial.SolveQuadric(t2,t1,t0);

            return ts;
            
            /*double dx=lin.DX; 
            double dy=lin.DY;
            double x0=lin.X1;
            double y0=lin.Y1;

            double t2=con.C*dy*dy+con.A*dx*dx;
            double t1=2*con.C*dy*y0+2*con.A*dx*x0+dy*con.E+con.D*dx;
            double t0=con.C*y0*y0+con.E*y0+con.A*x0*x0+con.D*x0+con.F;

            return RealPolynomial.SolveQuadric(t2, t1, t0, 1e-9);*/
        }

        /*private static Point2d[] ConicStandardPosLine(GeneralConic2d con, Line2d lin)
        {
            return LineParamsToPoints(ConicStandardPosLineParametric(con, lin), lin, double.NegativeInfinity, double.PositiveInfinity);
        }*/

        public static Point2d[] ConicLine(GeneralConic2d con, Line2d lin)
        {


            return LineParamsToPoints(ConicLineParametric(con,lin) , lin, double.NegativeInfinity, double.PositiveInfinity);


            /*
            Transform2d tr = con.ToStandardPosition;

            GeneralConic2d stdcon=new GeneralConic2d(con);
            Line2d stdlin=new Line2d(lin);
            stdcon.Transform(tr);
            stdlin.Transform(tr);

            double[] ts=ConicStandardPosLineParametric(stdcon,stdlin);

            return LineParamsToPoints(ts, lin, double.NegativeInfinity, double.PositiveInfinity);*/
        }


        

        #endregion

        #region PARABOLA_DOMINANT

        public static double[] ParabolaLineParamteric(Parabola2d pab,Line2d lin) { //tested ok

            double x1,y1,x2,y2; //line points in parabola standard position

            Transform2d tr=pab.ToStandardPosition; //intersect line with y=x^2 => easier
            tr.Apply(lin.X1,lin.Y1,out x1,out y1,true);
            tr.Apply(lin.X2,lin.Y2,out x2,out y2,true);

            double dx=x2-x1;
            double dy=y2-y1;
            double c2 = -dx * dx;
            double c1 = dy - 2 * dx * x1;
            double c0 = y1 - x1 *x1;
            //y1-x1^2+t*(dy-2*dx*x1)-dx^2*t^2=0

            double[] ts=RealPolynomial.SolveQuadric(c2, c1, c0);

            return ts;
        }

        public static Point2d[] ParabolaLine(Parabola2d pab, Line2d lin)
        {
            return LineParamsToPoints(ParabolaLineParamteric(pab, lin), lin, double.NegativeInfinity, double.PositiveInfinity);
        }


        /// <summary>
        /// Intersects a general conic with the curve y=x^2, returning a point set (possibly empty).
        /// This function is used as a helper function for parabola dominant intersection functions.
        /// </summary>
        private static Point2dSet StandardPosParabolaGeneralConic(GeneralConic2d con) { //tested ok

            //c*x^4+b*x^3+(e+a)*x^2+d*x+f
            Point2dSet res = new Point2dSet();
            double[] xs = GetRealRoots(con.C, con.B, con.E + con.A, con.D, con.F);
            if (xs == null) return res; //no solutions

            foreach (double x in xs)
            {
                double tx = x;
                double ty = x * x;
                res.Add(new Point2d(tx, ty));
            }
            
            return res;
        }

        public static Point2d[] ParabolaParabola(Parabola2d pab1,Parabola2d pab2) { //tested ok
            Transform2d tr=pab1.ToStandardPosition;
            pab2=new Parabola2d(pab2); //copy for transformation
            pab2.Transform(tr);
            var ptset = StandardPosParabolaGeneralConic(pab2.ToGeneralConic());
            ptset.Transform(tr.Inversed);
            return ptset.ToArray();
        }


        public static Point2d[] ParabolaHyperbola(Parabola2d pab, Hyperbola2d hyp) //tested ok
        {
            Transform2d tr = pab.ToStandardPosition; //y=x^2
            hyp = new Hyperbola2d(hyp); //copy for transformation
            hyp.Transform(tr);  //to standard space of parabola for stabillity

            GeneralConic2d gencon=hyp.ToGeneralConic();
            var ptset = StandardPosParabolaGeneralConic(gencon);
            ptset.Transform(tr.Inversed);
            return ptset.ToArray();
        }

        #endregion

    }
}
