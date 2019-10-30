using Guppy2.Calc;
using Guppy2.Calc.Geom2d;
using System;
using System.Collections.Generic;

namespace Guppy2.GFX
{


    public delegate void FlattenCallback(double x, double y,bool moveto);
    public delegate void LineCallback(double x0,double y0,double x1,double y1);
    public delegate void ArcCallback(double x0, double y0, double x1, double y1,double bulge);
    public delegate void BezierCallback(double x1,double y1,double x2,double y2,double x3,double y3,double x4,double y4);

    
    /// <summary>
    /// Ellipse and elliptic arc utility functions
    /// </summary>
    public static class GeomUtil
    {

        

        /// <summary>
        /// Transforms the parameters of an ellipse.
        /// Returns true if the resulting ellipse is considered a circle.
        /// </summary>
        public static bool TransformCentralEllipse(ref double majaxis, ref double minaxis, ref double tilt, Transform2d matrix, ref double sweepangle)
        {

            double s = Math.Sin(tilt);
            double c = Math.Cos(tilt);

            // build ellipse representation matrix (unit circle transformation).
            // the 2x2 matrix multiplication with the upper 2x2 of a_mat is inlined.
            double m0 = matrix.AX * majaxis * c + matrix.BX * majaxis * s;
            double m1 = matrix.AY * majaxis * c + matrix.BY * majaxis * s;
            double m2 = matrix.AX * -minaxis * s + matrix.BX * minaxis * c;
            double m3 = matrix.AY * -minaxis * s + matrix.BY * minaxis * c;

            // to implict equation (centered)
            double A = (m0 * m0) + (m2 * m2);
            double C = (m1 * m1) + (m3 * m3);
            double B = (m0 * m1 + m2 * m3) * 2.0;


            // precalculate distance A to C
            double ac = A - C;
            double A2 = 0;
            double C2 = 0;



            // convert implicit equation to angle and halfaxis:
            if (Math.Abs(B)<1e-6) //not tilted
            {
               // tilt = (A > C ? 0.0 : Math.PI * 0.5); //support majoraxis<minoraxis
                //rot = 0.0;

                if (MathUtil.IsZero(ac) || A > C)
                    tilt = 0;
                else
                    tilt = MathUtil.Deg90;  

                A2 = A;
                C2 = C;
            }
            else
            {
                if (Math.Abs(ac)<1e-6)
                {
                    A2 = A + B * 0.5;
                    C2 = A - B * 0.5;
                    tilt = (A > C ? Math.PI * 0.25 : Math.PI * 0.75); //support majoraxis<minoraxis
                    //  rot = Math.PI * 0.25;
                }
                else
                {
                    // Precalculate radical:
                    double K = 1.0 + B * B / (ac * ac);

                    // Clamp (precision issues might need this.. not likely, but better safe than sorry)
                    if (K < 0.0)
                        K = 0.0;
                    else
                        K = Math.Sqrt(K);

                    A2 = 0.5 * (A + C + K * ac);
                    C2 = 0.5 * (A + C - K * ac);
                    tilt = 0.5 * Math.Atan2(B, ac);
                }
            }

            // This can get slightly below zero due to rounding issues.
            // it's save to clamp to zero in this case (this yields a zero length halfaxis)
            if (A2 < 0.0)
                A2 = 0.0;
            else
                A2 = Math.Sqrt(A2);
            if (C2 < 0.0)
                C2 = 0.0;
            else
                C2 = Math.Sqrt(C2);


            // now A2 and C2 are half-axis:
            if (ac <= 0)
            {
                minaxis = A2;
                majaxis = C2;
                //rot += Math.PI;
            }
            else
            {
                minaxis = C2;
                majaxis = A2;
            }

            //check sign of upper 2x2 submatrix determinant to know if the ellipse is 'reversed'
            //by matrix mirroring component
            if (matrix.AX * matrix.BY - matrix.AY * matrix.BX < 0.0)
                sweepangle = -sweepangle;

            return Math.Abs(majaxis-minaxis)<=MathUtil.Epsilon;
        }


        /// <summary>
        /// Gets a point on a tilted ellipse centered at origo given an angle.
        /// </summary>
        /// <param name="a">Major radius</param>
        /// <param name="b">Minro radius</param>
        /// <param name="tilt">Tiltangle of ellipse</param>
        /// <param name="angle">Angle to point on ellipse from origo</param>
        public static void GetPointOnCentralEllipse(double a, double b, double tilt, double angle, out double x, out double y)
        {
            double param = EllipseAngleToParam(angle, a, b);
            double ca = Math.Cos(tilt);
            double sa = Math.Sin(tilt);
            double cv = Math.Cos(param);
            double sv = Math.Sin(param);
            x = cv * a * ca - sv * b * sa;
            y = sv * b * ca + cv * a * sa;
        }

        public static double EllipseAngleToParam(double ang, double majrad, double minrad)
        {
            //reverse implementation taken from mathworld:s ellipse section 58 with twists and turns for qudrant troubles with tan
            double res;

            ang = MathUtil.NormalizeAngle(ang);

            //take care of top/bottom tangents
            if (Equals(ang, MathUtil.Deg90))
                return MathUtil.Deg90;
            else if (Equals(ang, MathUtil.Deg270))
                return MathUtil.Deg270;
            if (MathUtil.IsZero(minrad))
                return ang < MathUtil.Deg180 ? MathUtil.Deg90 : MathUtil.Deg270;

            res = Math.Atan((majrad * Math.Tan(ang)) / minrad);

            if (ang > MathUtil.Deg90 && ang < MathUtil.Deg270) //correct for 2:nd or 3:d quadrant
                res += MathUtil.Deg180;

            return MathUtil.NormalizeAngle(res);
        }


        /// <summary>
        /// This is the function to use to transform a full ellipse with tilt given i radians.
        /// Returns if the resulting ellipse is a circle.
        /// </summary>
        public static bool TransformEllipse(ref double centerx,ref double centery,ref double majrad,ref double minrad,ref double tilt,Transform2d trans)
        {
            double sweepangle=0.0;
            bool res=TransformCentralEllipse(ref majrad, ref minrad, ref tilt, trans, ref sweepangle);
            trans.Apply(centerx, centery, out centerx, out centery,true);
            return res;
        }

       

        /// <summary>
        /// Transforms a general elliptical arc. Returns true if the transformed arc is a circular arc.
        /// </summary>
        public static bool TransformEllipseArc(ref double centerx, ref double centery, ref double aradius, ref double bradius,ref double startangle,ref double sweepangle, ref double tilt, Transform2d mat)
        {
            double endangle = startangle + sweepangle; //we do our calculations here with endangle rather than sweepangle

            mat.Apply(centerx, centery, out centerx, out centery, true);
            
            //get points on the untranformed ellipse and transform them to be
            //points on the central, tranformed ellipse
            double spx, spy, epx, epy;
            GeomUtil.GetPointOnCentralEllipse(aradius, bradius, tilt, startangle,out spx,out spy);
            GeomUtil.GetPointOnCentralEllipse(aradius, bradius, tilt, endangle, out epx, out epy);
            
            mat.Apply(spx, spy, out spx, out spy, false);
            mat.Apply(epx, epy, out epx, out epy, false);
            
            bool res=GeomUtil.TransformCentralEllipse(ref aradius, ref bradius, ref tilt, mat, ref sweepangle);

            startangle = Angle(0,0,spx,spy) - tilt;
            endangle = Angle(0,0,epx,epy) - tilt;
            
            //convert endangle back to startangle
            if (sweepangle > 0.0 && endangle < startangle) endangle += Math.PI * 2.0;
            else if (sweepangle < 0.0 && endangle > startangle) endangle -= Math.PI * 2.0;
            sweepangle = endangle - startangle;

            return res;
        }


        /// <summary>
        /// Computes the center point of an arc, given a bulge factor.
        /// Returns true on success or false if the arc is linear.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="bulge"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <returns></returns>
        public static bool GetArcCenterFromBulge(double x1, double y1, double x2, double y2, double bulge, out double cx, out double cy)
        {
            if (Math.Abs(bulge) < 1e-4) // (note: was Calc.Epsilon (1e-6) but this was hard to draw for some drivers since center=far away)
            {
                cx = cy = 0.0;
                return false; //linear=>no center
            }
            double fac = ((1.0 / bulge) - bulge) / 2.0;
            cx = ((x1 + x2) - (y2 - y1) * fac) / 2.0;
            cy = ((y1 + y2) + (x2 - x1) * fac) / 2.0;
            return true;
        }


        public static double GetArcRadiusFromBulge(double x1, double y1, double x2, double y2, double bulge)
        {
            return Math.Abs(Distance(x1,y1,x2,y2) * (bulge + 1.0 / bulge) / 4.0);
        }


        /// <summary>
        /// Returns the signed sweepangle for an arc given its bulge
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="bulge"></param>
        /// <returns></returns>
        public static double GetArcSweepAngleFromBulge(double bulge)
        {
            return 4.0 * Math.Atan(bulge);
        }

        /// <summary>
        /// Computes the bulge factor for an arc, given a signed sweep angle
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static double GetArcBulgeFromSweepAngle(double angle)
        {
            return Math.Tan(angle / 4.0);
        }

        /// <summary>
        /// Returns the distance between two points.
        /// </summary>
        public static double Distance(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double SquaredDistance(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return dx * dx + dy * dy;
        }
        

        /// <summary>
        /// Computes the plane angle between two points in the range 0.0 to 2*PI.
        /// </summary>
        public static double Angle(double x1, double y1, double x2, double y2)
        {
            double res = Math.Atan2(y2 - y1, x2 - x1);
            if (res < 0.0) res += MathUtil.Deg360;
            return res;
        }


        public static void FlattenArc(double x1, double y1, double x2, double y2, double bulge, bool firstpoint, double flattentolerance, FlattenCallback cb)
        {
            if (firstpoint) 
                cb(x1, y1,true);
            FlattenArcInternal(x1, y1, x2, y2, bulge, flattentolerance, cb);
        }

        public static void FlattenArcInternal(double x1, double y1, double x2, double y2, double bulge, double flattentolerance,FlattenCallback cb)
        {
            double chord = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
            double altitude = -chord * bulge * 0.5;

            if (Math.Abs(altitude) < flattentolerance)
            {
                cb(x2, y2,false);
                return;
            }

            double midx = (x2 + x1) * 0.5;
            double midy = (y2 + y1) * 0.5;

            double vx = x2 - x1;
            double vy = y2 - y1;
            double fac = altitude / chord;
            vx *= fac;
            vy *= fac;
            midx -= vy;
            midy += vx;

            //compute new bulge factor for half of arc:
            bulge = Math.Tan(0.5 * Math.Atan(bulge));
            
            FlattenArcInternal(x1, y1, midx, midy, bulge,flattentolerance,cb);
            FlattenArcInternal(midx, midy, x2, y2, bulge,flattentolerance,cb);
        }


        public static void FlattenBezier(double x1, double y1, double x2,double y2, double x3, double y3,double x4,double y4, bool firstpoint, double flattentolerance, FlattenCallback cb,double t1=0.0,double t2=1.0)
        {
            
            double sqrtol = flattentolerance * flattentolerance;
            double midt = (t1 + t2) * 0.5;

            //check if estimation line passes thru curve at parametric midpoint, if so split at middle t
            //otherwise just recurse:
            double midx,midy,t1x,t1y,t2x,t2y;

            if(t1==0.0) {
                t1x=x1;
                t1y=y1;
            }
            else 
                EvalBezier(x1,y1,x2,y2,x3,y3,x4,y4,t1,out t1x,out t1y);

            if (t2 == 1.0)
            {
                t2x = x4;
                t2y = y4;
            }
            else
                EvalBezier(x1, y1, x2, y2, x3, y3, x4, y4, t2, out t2x, out t2y);


            if (firstpoint)
                cb(t1x, t1y, true);


            EvalBezier(x1, y1, x2, y2, x3, y3, x4, y4, midt, out midx, out midy);

            double tdist = SquaredDistance((t1x + t2x) / 2.0, (t1y + t2y) / 2.0, midx, midy); 
            if (tdist <= sqrtol)
            {
                FlattenBezierInternal(x1, y1, x2, y2, x3, y3, x4, y4, t1, t1x, t1y, midt, midx, midy, sqrtol, cb);
                FlattenBezierInternal(x1, y1, x2, y2, x3, y3, x4, y4, midt, midx, midy, t2, t2x, t2y, sqrtol, cb);
            }
            else
            {
                FlattenBezierInternal(x1, y1, x2, y2, x3, y3, x4, y4, t1, t1x, t1y, t2, t2x, t2y, sqrtol, cb);
            }
        }

        public static void EvalBezier(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, double t,out double x,out double y)
        {
            double it = (1.0 - t);
            double it2 = it * it;
            double it3 = it * it * it;
            x = it3 * x1 + (3.0 * t * it2) * x2 + (3.0 * t * t * it) * x3 + (t * t * t) * x4;
            y = it3 * y1 + (3.0 * t * it2) * y2 + (3.0 * t * t * it) * y3 + (t * t * t) * y4;
        }

        static void FlattenBezierInternal(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4,
            double t1, double t1x, double t1y, double t2, double t2x, double t2y, double sqrtol, FlattenCallback cb)
        {
            double midx, midy, midt = (t1 + t2) / 2.0;
            EvalBezier(x1, y1, x2, y2, x3, y3, x4, y4, midt, out midx, out midy);
            if (SquaredDistance((t1x + t2x) / 2.0, (t1y + t2y) / 2.0, midx, midy) > sqrtol)
            {
                FlattenBezierInternal(x1, y1, x2, y2, x3, y3, x4, y4, t1, t1x, t1y, midt, midx, midy, sqrtol, cb);
                FlattenBezierInternal(x1, y1, x2, y2, x3, y3, x4, y4, midt, midx, midy, t2, t2x, t2y, sqrtol, cb);
            }

            cb(t2x,t2y,false);
        }

        /// <summary>
        /// Gets parameters from an arc given in bulge notation. returns true on success or false if
        /// the arc is linear.
        /// </summary>
        public static bool GetArcParams(double x1, double y1, double x2, double y2,double bulge,out double centerx,out double centery,out double radius,out double startangle,out double sweepangle)
        {
            if (GetArcCenterFromBulge(x1, y1, x2, y2, bulge, out centerx, out centery))
            {
                radius = GetArcRadiusFromBulge(x1, y1, x2, y2, bulge);
                sweepangle = GetArcSweepAngleFromBulge(bulge);
                startangle = Angle(centerx, centery, x1, y1);
                return true;
            }
            radius = startangle = sweepangle = 0.0;
            return false;
        }


        public static double PerpDot(double x1, double y1, double x2, double y2) { return x1*y2-y1*x2; }

        public static bool Intersect(double ax1, double ay1, double ax2, double ay2, double bx1, double by1, double bx2, double by2, out double xi, out double yi)
        {

            double denom = ax1 * (by2 - by1) + ax2 * (by1 - by2) + bx2 * (ay2 - ay1) + bx1 * (ay1 - ay2);
            if (Math.Abs(denom) > MathUtil.Epsilon)
            { //not paralell, s is intersection parameter on l1
                double s = (ax1 * (by2 - by1) + bx1 * (ay1 - by2) + bx2 * (by1 - ay1)) / denom;
                xi = ax1 + s * (ax2 - ax1);
                yi = ay1 + s * (ay2 - ay1);
                return true;
            }
            xi = yi = 0.0;
            return false;
        }

        public static void GetPointOnEllipseFromParam(double centerx,double centery,double a, double b, double tilt, double param, out double x, out double y)
        {
            double ca = Math.Cos(tilt);
            double sa = Math.Sin(tilt);
            double cv = Math.Cos(param);
            double sv = Math.Sin(param);
            x = cv * a * ca - sv * b * sa+centerx;
            y = sv * b * ca + cv * a * sa+centery;
        }

        public static void FlattenEllipticArc(double centerx, double centery, double aradius, double bradius, double tilt, double startangle, double sweepangle, double flattentol, bool firstpoint, FlattenCallback lcb)
        {
            
            //check if full ellipse
            if (Math.Abs(sweepangle) >= MathUtil.Deg360 - MathUtil.Epsilon)
            {
                FlattenEllipse(centerx, centery, aradius, bradius, tilt, flattentol, firstpoint, lcb);
                return;
            }

            double startparam = EllipseAngleToParam(startangle,aradius,bradius);
            double endparam = EllipseAngleToParam(startangle+sweepangle, aradius, bradius);
            double sweepparam;

            if (sweepangle > 0.0) //ccw
            {
                while (endparam < startparam)
                    endparam += MathUtil.Deg360;
                sweepparam = endparam - startparam; 
            }
            else
            { //cw
                while (endparam > startparam)
                    endparam -= MathUtil.Deg360;
                sweepparam = endparam - startparam;
            }
            
            //draw it as a transformed unit size arc
            double cst = Math.Cos(tilt);
            double sit = Math.Sin(tilt);
            Transform2d tr = new Transform2d(Math.Cos(tilt) * aradius, Math.Sin(tilt) * aradius, Math.Cos(tilt+MathUtil.Deg90) * bradius, Math.Sin(tilt+MathUtil.Deg90) * bradius, centerx, centery);
            double x1 = Math.Cos(startparam);
            double y1 = Math.Sin(startparam);
            double x2 = Math.Cos(endparam);
            double y2 = Math.Sin(endparam);
            double bulge = GetArcBulgeFromSweepAngle(sweepparam); //should work with negative sweeps as well
            TransformFlattenArc(x1, y1, x2, y2, bulge, firstpoint, flattentol, tr, lcb);
        }

        public static void FlattenEllipse(double centerx, double centery, double aradius, double bradius, double tilt, double flattentol,bool firstpoint, FlattenCallback lcb)
        {
            //draw it as a transformed unit size arcs
            double cst = Math.Cos(tilt);
            double sit = Math.Sin(tilt);
            Transform2d tr = new Transform2d(Math.Cos(tilt) * aradius, Math.Sin(tilt) * aradius, Math.Cos(tilt + MathUtil.Deg90) * bradius, Math.Sin(tilt + MathUtil.Deg90) * bradius, centerx, centery);
            TransformFlattenArc(1.0,0.0,-1.0,0.0,1.0, firstpoint, flattentol, tr, lcb);
            TransformFlattenArc(-1.0, 0.0, 1.0, 0.0, 1.0, false, flattentol, tr, lcb);
        }


        public static void GetEllipseFoci(double centerx,double centery,double aradius,double bradius,double tilt,out double xf1,out double yf1,out double xf2,out double yf2) {
            double c = Math.Sqrt(aradius * aradius - bradius * bradius);
            Polar(centerx,centery,tilt,c,out xf1,out yf1);
            Polar(centerx,centery,tilt,-c,out xf2,out yf2);
        }

        public static void Polar(double x,double y,double angle,double dist,out double rx,out double ry) {
            rx=x+Math.Cos(angle)*dist;
            ry=y+Math.Sin(angle)*dist;
        }


        public static void Bisector(double tipx, double tipy, double leg1x, double leg1y, double leg2x, double leg2y,out double bix,out double biy)
        {
            //bisectris theorem:
            double a = Distance(tipx,tipy,leg1x,leg1y);
            double b = Distance(tipx,tipy,leg2x,leg2y);
            double e = Distance(leg1x, leg1y, leg2x, leg2y);
            Polar(leg1x, leg1y, Angle(leg1x, leg1y, leg2x, leg2y), a * e / (a + b), out bix, out biy);
        }



        public static Point2d EvalEllipseParam(double cx, double cy, double aradius, double bradius, double tilt, double param)
        {
            double ca = Math.Cos(tilt);
            double sa = Math.Sin(tilt);
            double cv = Math.Cos(param);
            double sv = Math.Sin(param);
            return new Point2d(cv * aradius * ca - sv * bradius * sa + cx, sv * bradius * ca + cv * aradius * sa + cy);
        }

        /// <summary>
        /// Computes the angle that is perpendicular to the ellipse at a given point.
        /// </summary>
        public static double GetEllipsePerpAngle(double centerx, double centery, double aradius, double bradius, double tilt, double onellipx, double onellipy)
        {
            double focx1,focy1,focx2,focy2,perpx,perpy;
            GetEllipseFoci(centerx, centery, aradius, bradius, tilt, out focx1, out focy1, out focx2, out focy2);
            Bisector(onellipx, onellipy, focx1, focy1, focx2, focy2, out perpx, out perpy);
            return Angle(perpx, perpy, onellipx, onellipy);
        }


        

       

        public static Box2d GetEllipticExtents(double cx, double cy, double aradius, double bradius, double startangle, double sweepangle, double tilt)
        {
            Box2d extents = new Box2d();

            double vmaxx, vmaxy;

            if(MathUtil.IsZero(bradius)) //special case for zero bradius, assume maxima lies in tilt direction
                vmaxy=tilt;
            else
                vmaxy = Math.Atan(bradius / (aradius * Math.Tan(tilt)));

            if (MathUtil.IsZero(aradius)) //special case for zero bradius, assume maxima lies in tilt direction
                vmaxx=tilt;
            else
                vmaxx = -Math.Atan((bradius * Math.Tan(tilt)) / aradius);
            

            double startparam = EllipseAngleToParam(startangle, aradius, bradius);
            double endparam = EllipseAngleToParam(startangle+sweepangle, aradius, bradius);
            bool ccw = sweepangle >= 0.0;


            if (Math.Abs(sweepangle) < MathUtil.Deg360)
            {
                extents.Append(EvalEllipseParam(cx, cy, aradius, bradius, tilt, startparam));
                extents.Append(EvalEllipseParam(cx, cy, aradius, bradius, tilt, endparam));
                if(MathUtil.IsAngleBetween(startparam,endparam,vmaxy,ccw))
                    extents.Append(EvalEllipseParam(cx, cy, aradius, bradius, tilt, vmaxy));
                if(MathUtil.IsAngleBetween(startparam,endparam,vmaxy+MathUtil.Deg180,ccw))
                    extents.Append(EvalEllipseParam(cx, cy, aradius, bradius, tilt, vmaxy + MathUtil.Deg180));
                if(MathUtil.IsAngleBetween(startparam,endparam,vmaxx,ccw))
                    extents.Append(EvalEllipseParam(cx, cy, aradius, bradius, tilt, vmaxx));
                if(MathUtil.IsAngleBetween(startparam,endparam,vmaxx+MathUtil.Deg180,ccw))
                    extents.Append(EvalEllipseParam(cx, cy, aradius, bradius, tilt, vmaxx + MathUtil.Deg180));
            }
            else
            {
                //not an arc, all extreme ellipse points are included => a little faster
                extents.Append(EvalEllipseParam(cx, cy, aradius, bradius, tilt, vmaxy));
                extents.Append(EvalEllipseParam(cx, cy, aradius, bradius, tilt, vmaxy + MathUtil.Deg180));
                extents.Append(EvalEllipseParam(cx, cy, aradius, bradius, tilt, vmaxx));
                extents.Append(EvalEllipseParam(cx, cy, aradius, bradius, tilt, vmaxx + MathUtil.Deg180));
            }

            return extents;
        }


        public static Box2d GetArcExtents(double x1, double y1, double x2, double y2, double bulge)
        {
            double cx,cy,rad,side;
            Box2d res = new Box2d(x1, y1, x2, y2);

            //append quadrants if included

            
            if(!GetArcCenterFromBulge(x1, y1, x2, y2, bulge, out cx, out cy))
                return res; //arc is a line
            rad=GetArcRadiusFromBulge(x1,y1,x2,y2,bulge);

            //include quadrant points of circle if on arc
            side=SideOfPoint(x1, y1, x2, y2, cx+rad, cy);
            if ((bulge < 0.0 && side >= 0.0) || (bulge > 0.0 && side <= 0.0))
                res.Append(cx+rad,cy);
            
            side=SideOfPoint(x1, y1, x2, y2, cx, cy+rad);
            if ((bulge < 0.0 && side >= 0.0) || (bulge > 0.0 && side <= 0.0))
                res.Append(cx,cy+rad);
            
            side=SideOfPoint(x1, y1, x2, y2, cx-rad, cy);
            if ((bulge < 0.0 && side >= 0.0) || (bulge > 0.0 && side <= 0.0))
                res.Append(cx-rad,cy);
            
            side=SideOfPoint(x1, y1, x2, y2, cx, cy-rad);
            if ((bulge < 0.0 && side >= 0.0) || (bulge > 0.0 && side <= 0.0))
                res.Append(cx,cy-rad);

            return res;
        }

        public static Box2d GetTransformedArcExtents(double x1, double y1, double x2, double y2, double bulge,Transform2d t)
        {
            if (t.IsUniform)
            { //the transformed arc is still an arc
                t.Apply(x1, y1, out x1, out y1, true);
                t.Apply(x2, y2, out x2, out y2, true);
                return GetArcExtents(x1, y1, x2, y2, t.Determinant < 0.0 ? -bulge : bulge);
            }
            else
            {
                //the transformed arc is an elliptic arc,
                double cx,cy,arad,brad,startang,sweepang,tilt=0.0;
                GetArcParams(x1,y1,x2,y2,bulge,out cx,out cy,out arad,out startang,out sweepang);
                brad=arad;
                TransformEllipseArc(ref cx, ref cy, ref arad, ref brad, ref startang, ref sweepang, ref tilt, t);
                return GetEllipticExtents(cx, cy, arad, brad, startang, sweepang, tilt);
            }
        }

        

        public static Box2d GetBezierExtents(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            //equations for differentiations of the bezier polynoimial in x and y:
            double t2x = 9 * x2 - 9 * x3 + 3 * x4 - 3 * x1;
            double t1x = 6 * x1 - 12 * x2 + 6 * x3;
            double t0x = 3 * x2 - 3 * x1;

            double t2y = 9 * y2 - 9 * y3 + 3 * y4 - 3 * y1;
            double t1y = 6 * y1 - 12 * y2 + 6 * y3;
            double t0y = 3 * y2 - 3 * y1;

            double extrema1, extrema2, xp,yp;

            Box2d res = new Box2d(x1, y1, x4, y4);

            //solve x extremas
            if (MathUtil.SolveQuadratic(t2x, t1x, t0x, out extrema1, out extrema2))
            {
                if (extrema1 > 0.0 && extrema1 < 1.0)
                {
                    EvalBezier(x1, y1, x2, y2, x3, y3, x4, y4, extrema1, out xp, out yp);
                    res.Append(xp, yp);
                }

                if (extrema2 > 0.0 && extrema2 < 1.0)
                {
                    EvalBezier(x1, y1, x2, y2, x3, y3, x4, y4, extrema2, out xp, out yp);
                    res.Append(xp, yp);
                }
            }

            //solve y extremas
            if (MathUtil.SolveQuadratic(t2y, t1y, t0y, out extrema1, out extrema2))
            {
                if (extrema1 > 0.0 && extrema1 < 1.0)
                {
                    EvalBezier(x1, y1, x2, y2, x3, y3, x4, y4, extrema1, out xp, out yp);
                    res.Append(xp, yp);
                }

                if (extrema2 > 0.0 && extrema2 < 1.0)
                {
                    EvalBezier(x1, y1, x2, y2, x3, y3, x4, y4, extrema2, out xp, out yp);
                    res.Append(xp, yp);
                }
            }
            
            return res;
        }

        public static Box2d GetTransformedBezierExtents(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4,Transform2d t)
        {
            t.Apply(x1, y1, out x1, out y1, true);
            t.Apply(x2, y2, out x2, out y2, true);
            t.Apply(x3, y3, out x3, out y3, true);
            t.Apply(x4, y4, out x4, out y4, true);
            return GetBezierExtents(x1, y1, x2, y2, x3, y3, x4, y4);
        }



        /// <summary>
        /// Finds a parameter on the ellipse which has the given slope, in the interwal -pi/2 to pi/2.
        /// Slope is allowed to be +-infinity for the vertical line (parameter=0.0)
        /// </summary>
        public static double FindEllipseParamFromSlope(double slope, double arad, double brad)
        {
            //the math behind this is taking the implicit diffrentiation of ellipse x^2/a^2+y^2/b^2 and setting
            //it equal for the slope. For x,y in formula we use sin and cos of param we search for
            return Math.Atan((-brad * brad) / (slope * arad * arad));
        }


        public static double InfiniteLinePointDistance(double x1, double y1, double x2, double y2, double px, double py)
        {
            //see http://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
            /*double dx = x2 - x1;
            double dy = y2 - y1;
            
            double normlen = Math.Sqrt(dx * dx + dy * dy);
            if (normlen < 1e-20) return GeomUtil.Distance(x1, y1, px, py);
            return Math.Abs((px - x1) * dy - (py - y1) * dx) / normlen;*/

            double dx = x2 - x1;
            double dy = y2 - y1;
            double l2 = dx*dx+dy*dy;
            if (l2 < 1e-12)
                return 0.0;// Distance(x1, y1, px, py);
            double s = ((y1 - py) * (x2 - x1) - (x1 - px) * (y2 - y1)) / l2;
            return Math.Abs(s * Math.Sqrt(l2));

        }

        /// <summary>
        /// Returns posetive for left side, negative for right or 0 for on.
        /// </summary>
        public static double SideOfPoint(double p0x, double p0y, double p1x, double p1y, double x, double y)
        {
            return (p0x - p1x) * (p0y - y) - (p0y - p1y) * (p0x - x);
        }


        public static void TransformFlattenArc(double x1, double y1, double x2, double y2, double bulge, bool firstpoint, double tol, Transform2d t, FlattenCallback lcb)
        {
            
            if (firstpoint)
            {
                double tx1, ty1;    
                t.Apply(x1, y1, out tx1, out ty1, true);
                lcb(tx1, ty1, true);
            }

            bool uniform = t.IsUniform;
            if (t.IsUniform)
            {
                if (firstpoint)
                {
                    double tx1, ty1;
                    t.Apply(x1, y1, out tx1, out ty1, true);
                    lcb(tx1, ty1, true);
                }

                _TransformFlattenArcUniform(x1, y1, x2, y2, bulge, tol * tol, t, lcb);
            }
            else
            {
                double tx1, ty1,tx2,ty2;
                t.Apply(x1, y1, out tx1, out ty1, true);
                t.Apply(x2, y2, out tx2, out ty2, true); //pre-compute transformed points

                if (firstpoint)
                    lcb(tx1, ty1, true);

                InternalTransformFlattenArcNonUniform(x1, y1, x2, y2, bulge, tx1, ty1, tx2, ty2, tol * tol, t, lcb);
            }
        }

        private static void _TransformFlattenArcUniform(double x1, double y1, double x2, double y2, double bulge, double sqrtol, Transform2d t, FlattenCallback cb)
        {
            //vector from middle of chord to point on arc:
            double midvx = (y2 - y1) * bulge * 0.5;
            double midvy = -(x2 - x1) * bulge * 0.5;

            //transformed error vector:
            double errvx, errvy, midx, midy;
            t.Apply(midvx, midvy, out errvx, out errvy, false); //transform error vector

            if (errvx * errvx + errvy * errvy < sqrtol)
            {
                t.Apply(x2, y2, out x2, out y2, true);
                cb(x2, y2, false);
                return;
            }

            midx = (x2 + x1) * 0.5 + midvx;
            midy = (y2 + y1) * 0.5 + midvy;



            //compute new bulge factor for half of arc:
            bulge = Math.Tan(0.5 * Math.Atan(bulge));
            _TransformFlattenArcUniform(x1, y1, midx, midy, bulge, sqrtol, t,  cb);
            _TransformFlattenArcUniform(midx, midy, x2, y2, bulge, sqrtol, t,  cb);
        }


        private static void InternalTransformFlattenArcNonUniform(double x1, double y1, double x2, double y2, double bulge, double tx1, double ty1, double tx2, double ty2, double sqrtol, Transform2d t, FlattenCallback cb)
        {
            //tx1,ty1,tx2,ty2 is x1,y1,x2,y2 transformed (to save computation time we dont recompute thoose evry recursion)


            //vector from middle of chord to point on arc:
            double midvx = (y2 - y1) * bulge * 0.5;
            double midvy = -(x2 - x1) * bulge * 0.5;

            //transformed error vector:
            double errvx, errvy, midx, midy;
            t.Apply(midvx, midvy, out errvx, out errvy, false); //transform error vector

            //midpoint on non-transformed arc
            midx = (x2 + x1) * 0.5 + midvx;
            midy = (y2 + y1) * 0.5 + midvy;


            //non uniform transform: must check error point on (ellipse):s distance to the chordline:
            double tmx, tmy;    //transformed midpoint
            t.Apply(midx, midy, out tmx, out tmy, true);
            double err = InfiniteLinePointDistance(tx1, ty1, tx2, ty2, tmx, tmy);

            if (err * err < sqrtol) 
            {
                cb(tx2, ty2, false);
                return;
            }

            //compute new bulge factor for half of arc:
            bulge = Math.Tan(0.5 * Math.Atan(bulge));
            InternalTransformFlattenArcNonUniform(x1, y1, midx, midy, bulge, tx1, ty1, tmx, tmy, sqrtol, t, cb);
            InternalTransformFlattenArcNonUniform(midx, midy, x2, y2, bulge, tmx, tmy, tx2, ty2, sqrtol, t, cb);
        }


        public static void GetArcMidPoint(double x1,double y1,double x2,double y2,double bulge,out double midx,out double midy) {
            midx = 0.5 * (bulge * (y2 - y1) + x1 + x2);
            midy = 0.5 * (bulge * (x1 - x2) + y1 + y2);
        }


        /// <summary>
        /// Converts an arc which has a sweep angle less than 90 degrees into a single bezier curve.
        /// </summary>
        private static void SmallArcToBezier(double cenx, double ceny, double radius, double startangle, double endangle, out double x1, out double y1, out double xctl1, out double yctl1, out double xctl2, out double yctl2, out double x2, out double y2)
        {
            double halfLength, arcLength, alfa;

            arcLength = endangle - startangle;
            halfLength = (endangle - startangle) / 2;
            double s = Math.Tan(halfLength);
            alfa = Math.Sin(arcLength) * (Math.Sqrt(4 + 3 * s * s) - 1) / 3;

            // Start point
            double orgx1 = radius * Math.Cos(startangle);
            double orgy1 = radius * Math.Sin(startangle);

            // End point
            double orgx2 = radius * Math.Cos(endangle);
            double orgy2 = radius * Math.Sin(endangle);

            
            //set result
            x1 = orgx1 + cenx;
            y1 = orgy1 + ceny;

            
            xctl1 = orgx1 - alfa * orgy1+cenx;
            yctl1 = orgy1 + alfa * orgx1+ceny;

            xctl2 = orgx2 + alfa * orgy2+cenx;
            yctl2 = orgy2 - alfa * orgx2+ceny;

            x2 = orgx2 + cenx;
            y2 = orgy2 + ceny;
        }

      


        /// <summary>
        /// Appends controlpoints and endpoints to build an approximation to a list of x,y coordinates.
        /// Tha first point (x1,y1) is not appended, so the list is:
        /// xcontrol1,ycontrol1,xcontrol2,ycontrols,x,y,xcontrol1,ycontrol1,xcontrol2,ycontrols,x,y...
        /// 
        /// If the arc is a line, only x,y (endpoint of line) is appended
        /// </summary>
        public static void ArcToBeziers(double x1, double y1, double x2, double y2, double bulge,List<double> xy)
        {
            //recursivly divide arc to be less than 90 degrees to get a good arc approximation
            double centerx,centery;
            if(!GetArcCenterFromBulge(x1,y1,x2,y2,bulge,out centerx,out centery)) {
                //arc is a line, we just add end point of line in this case
                xy.Add(x2);
                xy.Add(y2);
                return;
            }

            double rad=GetArcRadiusFromBulge(x1, y1, x2, y2, bulge);

            double sweepang = Math.Abs(GetArcSweepAngleFromBulge(bulge));
            if (sweepang > MathUtil.Deg90)
            {
                double midx, midy;
                double subbulge = Math.Tan(0.5 * Math.Atan(bulge));  //bulge of half of arc
                GetArcMidPoint(x1, y1, x2, y2, bulge, out midx, out midy);
                ArcToBeziers(x1, y1, midx, midy, subbulge, xy);
                ArcToBeziers(midx, midy, x2, y2, subbulge, xy);
                return;
            }
            
            double sx,sy,ctlx1,ctly1,ctlx2,ctly2,ex,ey;
            //SmallArcToBezierControlPoints(x1, y1, x2, y2, centerx, centery,bulge, out ctlx1, out ctly1, out ctlx2, out ctly2);
            SmallArcToBezier(centerx, centery, rad, Angle(centerx, centery, x1, y1), Angle(centerx, centery, x2, y2), out sx, out sy, out ctlx1, out ctly1, out ctlx2, out ctly2, out ex, out ey);
            xy.Add(ctlx1);
            xy.Add(ctly1);
            xy.Add(ctlx2);
            xy.Add(ctly2);
            xy.Add(x2); //same as ex
            xy.Add(y2); //same as ey
        }


        public static void EllipticArcToBeziers(double cx, double cy, double aradius, double bradius, double tilt, double startangle, double sweepangle,List<double> xy)
        {
            int cnt=xy.Count;

            double startpar = EllipseAngleToParam(startangle,aradius,bradius);
            double endpar = EllipseAngleToParam(startangle+sweepangle, aradius, bradius);


            if (sweepangle >= MathUtil.Deg360)
            { //full ellipse
                endpar = startpar + MathUtil.Deg360;
            }
            else if (sweepangle <= -MathUtil.Deg360)
            {
                endpar = startpar - MathUtil.Deg360;
            }
            else if (sweepangle > 0.0) //ccw
            {
                if (endpar <= startpar) endpar += MathUtil.Deg360;
            }
            else //cw
            {
                if (endpar >= startpar) endpar -= MathUtil.Deg360;
            }
            double sweepparam = endpar - startpar;
            double bulge = GetArcBulgeFromSweepAngle(sweepparam);
            
            ArcToBeziers(Math.Cos(startpar), Math.Sin(startpar), Math.Cos(endpar), Math.Sin(endpar),bulge,xy);

            //transform bezier circular arc to elliptical arc
            Transform2d tr=Transform2d.Stretch(aradius,bradius)*Transform2d.Rotate(tilt)*Transform2d.Translate(cx,cy);
            for (int l = cnt; l < xy.Count; l += 2)
            {
                double tx, ty;
                tr.Apply(xy[l], xy[l + 1], out tx,out ty, true);
                xy[l] = tx;
                xy[l + 1] = ty;
            }
        }
    }
}
