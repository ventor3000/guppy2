using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{
    // The tem problems of appolonius are:
    // PPP (complete)
    // LPP (complete)
    // CPP (complete)
    // LLP (complete)
    // CLP (complete)
    // CCP (complete)
    // LLL (complete)
    // CLL (complete)
    // CCL (complete)
    // CCC (complete, maybe needs some simplifying)

    // TODO: make use of MakeResult in all functions
    // TODO: use TTTFilter for filtering objects.

    
    /// <summary>
    /// Flags which objects are reported as solutions to TanTanTan solvers.
    /// </summary>
    [Flags]
    public enum TTTFilter
    {
        /// <summary>
        /// Result contains circles inside or to the left of object 1
        /// </summary>
        Internal1 = 1,
        /// <summary>
        /// Result contains circles outside or to the right of object 1
        /// </summary>
        External1 = 2,
        /// <summary>
        /// Result contains circles inside or to the left of object 2
        /// </summary>
        Internal2 = 4,
        /// <summary>
        /// Result contains circles outside or to the right of object 2
        /// </summary>
        External2 = 8,
        /// <summary>
        /// Result contains circles inside or to the left of object 3
        /// </summary>
        Internal3 = 16,
        /// <summary>
        /// Result contains circles outside or to the right of object 3
        /// </summary>
        External3 = 32,
        /// <summary>
        /// Result contains all possible solutions
        /// </summary>
        All = 0xFFFF
    }

    public static class TanTanTanCircle
    {
        const double maxradius=1e10;  //larger circles than this are not added to result
        const double minradius = MathUtil.Epsilon; //smaller circles than this are not added to result
        
        public static Circle2d PointPointPoint(Point2d p1, Point2d p2, Point2d p3)
        {
            //see 'a programmers geometry' page 65
            double xlk = p2.X - p1.X;
            double ylk = p2.Y - p1.Y;
            double xmk = p3.X - p1.X;
            double ymk = p3.Y - p1.Y;
            double det = xlk * ymk - xmk * ylk;

            if (MathUtil.IsZero(det)) return null; //at least two points are coincident

            det = 0.5 / det;
            double rlksq = xlk * xlk + ylk * ylk;
            double rmksq = xmk * xmk + ymk * ymk;
            double xcc = det * (rlksq * ymk - rmksq * ylk);
            double ycc = det * (xlk * rmksq - xmk * rlksq);
            double rccsq = xcc * xcc + ycc * ycc;

            return new Circle2d(xcc + p1.X, ycc + p1.Y,Math.Sqrt(rccsq));
        }

        public static Circle2d[] LineLineLine(Line2d li1,Line2d li2,Line2d li3) {
            // see http://www.arcenciel.co.uk/geometry/ for explanation
            List<Circle2d> result = null;
            double a1,b1,c1,a2,b2,c2,a3,b3,c3,rad,u,v,xcenter,ycenter,denom;
            double t1 = 1.0, t2 = 1.0, t3 = 1.0;

            if(!li1.ToEquation(out a1,out b1,out c1)) return null;
            if(!li2.ToEquation(out a2,out b2,out c2)) return null;
            if(!li3.ToEquation(out a3,out b3,out c3)) return null;

            //line 2 must not be vertical for this solver, if so, exchange lines
            if (MathUtil.IsZero(b2))
            {
                MathUtil.Swap(ref a1, ref a2);
                MathUtil.Swap(ref b1, ref b2);
                MathUtil.Swap(ref c1, ref c2);
            }

            u=a2*b1-a1*b2;
            v=a3*b2-a2*b3;

            //loop for all 8 possible solutions (only 4 can actually be real solutions)
            for (int signcase = 0; signcase < 8; signcase++)
            {
                t1 = ((signcase & 1) == 0) ? -1 : 1;
                t2 = ((signcase & 2) == 0) ? -1 : 1;
                t3 = ((signcase & 4) == 0) ? -1 : 1;

                //compute radius
                denom = (v * (b1 * t2 - b2 * t1) - u * (b2 * t3 - b3 * t2));
                if (MathUtil.IsZero(denom)) continue;
                rad = (u * (b3 * c2 - b2 * c3) - v * (b2 * c1 - b1 * c2)) / denom;
                if (rad < minradius || rad>maxradius) continue;

                //compute center x
                if (!MathUtil.IsZero(u)) xcenter = (b2 * c1 - b2 * rad * t1 - b1 * c2 + b1 * rad * t2) / u;
                else if (!MathUtil.IsZero(v)) xcenter = (b3 * c2 + b3 * rad * t2 - b2 * c3 + b2 * rad * t3) / v;
                else continue;

                //compute center y
                if (b1 != 0.0) ycenter = (-a1 * xcenter - c1 + rad * t1) / b1;
                else if (b2 != 0.0) ycenter = (-a2 * xcenter - c2 + rad * t2) / b2;
                else ycenter = (-a3 * xcenter - c3 + rad * t3) / b3;

                AddResult(ref result, xcenter, ycenter, rad);
            } //end loop signcase

            if (result == null) return null;
            return result.ToArray();
        }

        public static Circle2d[] CircleLineLine(Circle2d ci,Line2d l1,Line2d l2) {
            
            // see http://www.arcenciel.co.uk/geometry/ for explanation

            List<Circle2d> result=null;

            //translate everyting so circle center at origo
            double dx=ci.X,dy=ci.Y;
            ci=new Circle2d(0,0,ci.Radius);
            l1=new Line2d(l1.X1-dx,l1.Y1-dy,l1.X2-dx,l1.Y2-dy);
            l2=new Line2d(l2.X1-dx,l2.Y1-dy,l2.X2-dx,l2.Y2-dy);
            
            //if first line vertical, swap lines...
            if(MathUtil.Equals(l1.X1,l1.X2)) {var tmp=l1;l1=l2;l2=tmp;}

            //if first line still vertical, special case:
            if (MathUtil.Equals(l1.X1, l1.X2))
            {
                double rad = (l1.X1 - l2.X1) / 2.0;
                double xcenter = (l1.X1 + l2.X1) / 2.0;

                double yc = Math.Sqrt((rad + ci.Radius) * (rad + ci.Radius) - xcenter * xcenter);

                AddResult(ref result, xcenter, ci.Y + yc, rad);
                AddResult(ref result, xcenter, ci.Y - yc, rad);
            }
            else
            {
                //now we know that first line is not vertical, and circle is centered at origo
                double a1, b1, c1, a2, b2, c2, u, w, s, a, b, c, xcenter, ycenter, t1, t2, r3;

                if (!l1.ToEquation(out a1, out b1, out c1)) return null;
                if (!l2.ToEquation(out a2, out b2, out c2)) return null;

                for (int signcase = 0; signcase < 8; signcase++)
                {

                    t1 = ((signcase & 1) == 0) ? -1 : 1;
                    t2 = ((signcase & 2) == 0) ? -1 : 1;
                    r3 = ((signcase & 4) == 0) ? -ci.Radius : ci.Radius;

                    u = (t1 * b2) - (t2 * b1);
                    w = (b1 * c2) - (b2 * c1);
                    s = (a1 * b2) - (a2 * b1);
                    a = (u * u) - (2 * a1 * s * u * t1) + (t1 * t1 * s * s) - (b1 * b1 * s * s);
                    b = 2.0 * ((u * w) + (c1 * a1 * s * u) - (a1 * s * t1 * w) - (c1 * t1 * s * s) - (r3 * b1 * b1 * s * s));
                    c = (w * w) + (2 * a1 * s * c1 * w) + (c1 * c1 * s * s) - (b1 * b1 * r3 * r3 * s * s);
                    double[] roots = RealPolynomial.SolveQuadric(a, b, c);
                    if (roots != null)
                    {
                        foreach (double radius in roots)
                        {

                            if (radius < minradius || radius > maxradius) continue;

                            if (!MathUtil.IsZero(s))
                            { //non parallel lines, one center per root
                                xcenter = (radius * u + w) / s;
                                ycenter = ((-a1 * xcenter) - c1 + (radius * t1)) / b1;
                                AddResult(ref result, xcenter, ycenter, radius);
                            }
                            else  //parallel lines, two centers per root
                            {
                                a = t1 * t1;
                                b = 2.0 * a1 * (c1 - (radius * t1));
                                c = ((radius * t1) - c1) * ((radius * t1) - c1) - (b1 * b2 * (r3 + radius) * (r3 + radius));
                                double[] roots2 = RealPolynomial.SolveQuadric(a, b, c);
                                if (roots2 != null)
                                {
                                    foreach (double x in roots2)
                                    {
                                        ycenter = (-a1 * x - c1 + radius * t1) / b1;
                                        AddResult(ref result, x, ycenter, radius);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        

            //translate results back to original position
            if (result != null)
            {
                foreach (Circle2d c in result)
                {
                    c.X += dx;
                    c.Y += dy;
                }
                return result.ToArray();
            }

            return null;
        }

        public static Circle2d[] CircleCircleLine(Circle2d ci1, Circle2d ci2, Line2d li)
        {
            // see http://www.arcenciel.co.uk/geometry/ for explanation

            List<Circle2d> result = null;
            double a1, b1, c1, t, r2, r3, a, b, c, u, s;
            double A, B, C, xc, yc;

            //transform input so that c1 is at origo and c2 is on xaxis
            Transform2d trans = Transform2d.Translate(Point2d.Origo - ci1.Center)*Transform2d.Rotate(-ci1.Center.Angle(ci2.Center));
            ci1=new Circle2d(ci1);
            ci1.Transform(trans);
            ci2 = new Circle2d(ci2);
            ci2.Transform(trans);
            li=new Line2d(li);
            li.Transform(trans);


            if (!li.ToEquation(out a1, out b1, out c1))
                return null; //degenerate line

            for (int signcase = 0; signcase < 8; ++signcase)
            {

                t = ((signcase & 1) == 0) ? 1 : -1;

                r2 = ((signcase & 2) == 0) ? ci1.Radius : -ci1.Radius;

                r3 = ((signcase & 4) == 0) ? ci2.Radius : -ci2.Radius;



                // Get constants
                a = 2 * (a1 * (r2 - r3) - ci2.X * t);
                b = 2 * b1 * (r2 - r3);
                c = 2 * c1 * (r2 - r3) + t * (r2 * r2 - r3 * r3 + ci2.X * ci2.X);

                if (!MathUtil.IsZero(b))
                {
                    u = b1 * c - b * c1;
                    s = a1 * b - a * b1;
                    A = t * t * b * b * (a * a + b * b) - b * b * s * s;
                    B = 2 * (u * t * b * (a * a + b * b) + a * c * s * t * b - b * b * s * s * r2);
                    C = u * u * (a * a + b * b) + 2 * a * c * s * u + c * c * s * s - b * b * s * s * r2 * r2;
                }
                else
                {
                    u = a1 * c - a * c1;
                    s = a * b1;
                    A = a * a * (t * t * a * a - s * s);
                    B = 2 * a * a * (u * t * a - s * s * r2);
                    C = u * u * a * a + c * c * s * s - a * a * s * s * r2 * r2;
                }



                // Calculate radius
                double[] roots = RealPolynomial.SolveQuadric(A, B, C);
                if (roots != null)
                {
                    foreach (double radius in roots)
                    {
                        if (radius < minradius || radius > maxradius)
                            continue;

                        // compute x coordinates of centers
                        List<double> xsols = new List<double>();

                        if (!MathUtil.IsZero(ci2.X)) //circles are not concentric
                        {
                            xc = ((r2 + radius) * (r2 + radius) - (r3 + radius) * (r3 + radius) + ci2.X * ci2.X) / (2 * ci2.X);
                            xsols.Add(xc);
                        }
                        else // If circles are concentric there can be 2 solutions for x
                        {
                            A = (a1 * a1 + b1 * b1);
                            B = -2 * a1 * (radius * t - c1);
                            C = (radius * t - c1) * (radius * t - c1) - b1 * b1 * (r2 + radius) * (r2 + radius);

                            double[] roots2 = RealPolynomial.SolveQuadric(A, B, C);
                            if (roots2 != null)
                            {
                                foreach (double x in roots2)
                                    xsols.Add(x);
                            }
                        }



                        // now compute y coordinates from the calculated x:es
                        // and input the final solution
                        foreach (double x in xsols)
                        {
                            if (!MathUtil.IsZero(b1))
                                yc = (-a1 * x - c1 + radius * t) / b1;
                            else
                            {
                                double ycSquare = (r2 + radius) * (r2 + radius) - (x * x);
                                if (ycSquare < 0.0) continue;
                                yc = Math.Sqrt(ycSquare);
                            }

                            AddResult(ref result, x, yc, radius);
                            if (MathUtil.IsZero(b1))
                                AddResult(ref result, x, -yc, radius);
                        }
                    }
                }
            }


            //convert back to original coordinate system by using the inverse
            //of the original matrix
            if (result != null)
            {
                trans = trans.Inversed;
                for (int l = 0; l < result.Count; l++)
                    result[l].Transform(trans);
                return result.ToArray();
            }

            return null;
        }

        public static Circle2d[] CircleCircleCircle(Circle2d ci1, Circle2d ci2, Circle2d ci3)
        {

            // see http://www.arcenciel.co.uk/geometry/ for explanation

            List<Circle2d> result = null;
            double r1, r2, r3, a, b, c, t, A, B, C;
            double fRadius, xc, yc, distc1c2;
            double[] roots;

            // if all circles concentric, there are no solutions
            distc1c2 = ci1.Center.Distance(ci2.Center);
            if (MathUtil.IsZero(distc1c2) && MathUtil.IsZero(ci2.Center.Distance(ci3.Center))) return null;

            // make sure first 2 circles are not concentric
            // if so swap ci2,ci3
            if (MathUtil.IsZero(distc1c2)) {var tmp = ci2;ci2 = ci3;ci3 = ci2;}

            
            // transform input so that ci1 is at origo and ci2 is on xaxis
            Transform2d trans = Transform2d.Translate(Point2d.Origo - ci1.Center) * Transform2d.Rotate(-ci1.Center.Angle(ci2.Center));
            ci1 = new Circle2d(ci1);
            ci1.Transform(trans);
            ci2 = new Circle2d(ci2);
            ci2.Transform(trans);
            ci3 = new Circle2d(ci3);
            ci3.Transform(trans);


            


            // Negate the radii to get all combinations

            for (int iCase = 0; iCase < 8; ++iCase)
            {
                r1 = ((iCase & 1) == 0) ? ci1.Radius : -ci1.Radius;
                r2 = ((iCase & 2) == 0) ? ci2.Radius : -ci2.Radius;
                r3 = ((iCase & 4) == 0) ? ci3.Radius : -ci3.Radius;

                // special case where radii of first 2 circles are equal
                if (MathUtil.Equals(r1, r2))
                {
                    // Calculate x-cordinate of centre
                    xc = ci2.X / 2.0;

                    // if all radii are equal, there will be only one solution
                    if (MathUtil.Equals(r1, r3))
                    {
                        if (MathUtil.IsZero(ci3.Y)) continue;

                        // get y-coordinate of centre
                        yc = (ci3.X * ci3.X - 2.0 * xc * ci3.X + ci3.Y * ci3.Y) / (ci3.Y + ci3.Y);

                        // compute radius
                        A = 1;
                        B = 2 * r1;
                        C = r1 * r1 - xc * xc - yc * yc;
                        roots = RealPolynomial.SolveQuadric(A, B, C);

                        if (roots.Length > 0)
                        {
                            fRadius = roots[0];
                            if (fRadius <= 0.0)
                            { //then try other root
                                if (roots.Length > 1)
                                {
                                    fRadius = roots[1];
                                    if (fRadius <= 0.0)
                                        continue; //no posetive roots
                                }
                            }
                            AddResult(ref result, xc, yc, fRadius);
                        }
                    }
                    else
                    {
                        // compute constants
                        double k = r1 * r1 - r3 * r3 + ci3.X * ci3.X + ci3.Y * ci3.Y - 2 * xc * ci3.X;
                        A = 4 * ((r1 - r3) * (r1 - r3) - ci3.Y * ci3.Y);
                        B = 4 * (k * (r1 - r3) - 2 * ci3.Y * ci3.Y * r1);
                        C = 4 * xc * xc * ci3.Y * ci3.Y + k * k - 4 * ci3.Y * ci3.Y * r1 * r1;

                        if (!MathUtil.IsZero(A))
                        {

                            roots = RealPolynomial.SolveQuadric(A, B, C);

                            foreach (double radius in roots)
                            {
                                yc = (2 * radius * (r1 - r3) + k) / (2 * ci3.Y);
                                AddResult(ref result, xc, yc, radius);
                            }
                        }

                    }
                    continue;
                } //end special case of r1==r2



                // Get constants
                a = 2 * (ci2.X * (r3 - r1) - ci3.X * (r2 - r1));
                b = 2 * ci3.Y * (r1 - r2);
                c = (r2 - r1) * (ci3.X * ci3.X + ci3.Y * ci3.Y - (r3 - r1) * (r3 - r1)) - (r3 - r1) * (ci2.X * ci2.X - (r2 - r1) * (r2 - r1));
                t = (ci2.X * ci2.X + r1 * r1 - r2 * r2) / 2.0;
                A = (r1 - r2) * (r1 - r2) * (a * a + b * b) - (ci2.X * ci2.X * b * b);
                B = 2 * (t * (r1 - r2) * (a * a + b * b) + a * c * ci2.X * (r1 - r2) - (r1 * ci2.X * ci2.X * b * b));
                C = t * t * (a * a + b * b) + (2 * a * c * ci2.X * t) + (c * c * ci2.X * ci2.X) - (r1 * r1 * ci2.X * ci2.X * b * b);

                // Calculate radius
                roots = RealPolynomial.SolveQuadric(A, B, C);
                if (roots == null) continue;

                foreach (double radius in roots)
                {
                    if (radius < minradius || radius > maxradius) continue;
                    // get x coordinate of centre (x2 may not be zero)
                    xc = (radius * (r1 - r2) + t) / ci2.X;

                    // get y coordinate of centre. b should never be 0, as 
                    // r1=r2 is special case and y3 may not be zero
                    yc = (-a * xc - c) / b;
                    AddResult(ref result, xc, yc, radius);
                }
            }

            //convert back to original coordinate system by using the inverse
            //of the original matrix
            if (result != null)
            {
                trans = trans.Inversed;
                for (int l = 0; l < result.Count; l++)
                    result[l].Transform(trans);
                return result.ToArray();
            }

            return null;
        }

        public static Circle2d[] CirclePointPoint(Circle2d ci, Point2d pt1, Point2d pt2)
        {

            // Solution by Robert.P. 2013-02-10
            // solved by intersecting the line which has equal distance to pt1 and pt2 at every point,
            // with the hyperbola that has equal distance to ci and pt1 at every point.


            List<Circle2d> res=null;

            //get squared constants of hyperbola
            double c2 = ci.Center.SquaredDistance(pt1)/4.0; //center->focus distance squared
            
            double a = 0.5*ci.Radius;
            double b = Math.Sqrt(c2 - a * a);
            if (double.IsNaN(b))
                return null;    //negative square root, no such hyperbola

            // this hyperbola has equal distance to ci and pt1 at every point.
            // it also has focus at ci.Center and pt1
            Hyperbola2d hyper = new Hyperbola2d(new Point2d((ci.X + pt1.X) * 0.5, (ci.Y + pt1.Y) * 0.5),a,b,ci.Center.Angle(pt1));
            
            //get constants of line dx+ey+f=0, which is equal distance from p1,p2 on every point
            double dx = pt2.X - pt1.X, dy = pt2.Y - pt1.Y;
            double mx = pt1.X + dx * 0.5;
            double my = pt1.Y + dy * 0.5;
            Line2d perp=new Line2d(mx,my,mx-dy,my+dx);    //starts from mipoint pt1-pt2 and is perpendicular to that line
            
            var intpts = Intersect2d.HyperbolaLine(hyper, perp);

            if (intpts == null)
                return null;

            foreach (Point2d intpt in intpts)
                AddResult(ref res, intpt.X, intpt.Y, intpt.Distance(pt1));

            return MakeResult(res);
        }


        public static Circle2d[] LineLinePoint(Line2d lin1, Line2d lin2, Point2d pnt)
        {
            // Compute the 2 circles (or 1 in special cases) that tangents two lines and a point.
            // Solving for parameter t on the bisector vector between lines that have the same
            // distance to point as the radius for that t (given using radfac computed below)
            // Solution by Robert.P. 2013-02-14

            List<Circle2d> res=null;

            Vector2d v1=lin1.Direction;
            Vector2d v2=lin2.Direction;
            Vector2d midvec;
            Point2d linint;
            double[] ts;

            

            linint = Intersect2d.LineLineAsPoint(lin1, lin2); //basepoint for midline vector
            
            if (linint == null) //special case for parallel lines
            {
                //use vector and base point on midpoint of radical line of circles
                midvec = v1;
                Line2d radlin = new Line2d(lin1.ClosestPoint(pnt), lin2.ClosestPoint(pnt));
                linint = radlin.MidPoint;
                double r = radlin.Length * 0.5, i = pnt.X, j = pnt.Y, x0 = linint.X, y0 = linint.Y, dx = midvec.X, dy = midvec.Y;

                double tp2 = 1; //squared length of midlin => assume normalized
                double tp1 = 2 *( dy * y0 + dx * x0 - dy * j - dx * i);
                double tp0 = y0 * y0 - 2 * j * y0 - r * r + j * j + i * i + x0 * x0 - 2 * i * x0;

                ts = RealPolynomial.SolveQuadric(tp2, tp1, tp0, 0.0);
                if (ts == null) return null;

                foreach (double t in ts)
                {
                    Point2d center = linint + midvec * t;
                    AddResult(ref res, center.X, center.Y, r);
                }
            }
            else //normal case with non-parallel lines
            {
                if (pnt.Location(lin1.Start, lin1.End) == pnt.Location(lin2.Start, lin2.End))
                    v2 = -v2;   //select correct bisector out of two (actually 4 but it does not matter if we solve for negative t)
                
                midvec = v1.Bisect(v2);
                double radfac = Math.Sin(v1.AngleTo(v2) / 2.0); //multiplied with midvector t this gives radius of circle at that t

                //solve problem in space where pnt=origo
                Point2d org = new Point2d(linint.X - pnt.X, linint.Y - pnt.Y);
                double t2 = -radfac * radfac + 1; //// 1 for squared length of midlin => assume normalized
                double t1 = 2 * (midvec.Y * org.Y + midvec.X * org.X);
                double t0 = org.X * org.X + org.Y * org.Y;
                ts = RealPolynomial.SolveQuadric(t2, t1, t0, 0.0);

                if (ts == null)
                    return null;

                foreach (double t in ts)
                {
                    Point2d center = linint + midvec * t;
                    AddResult(ref res, center.X, center.Y, Math.Abs(t * radfac));
                }
            }

            return MakeResult(res);
        }
        
        
        public static Circle2d[] LinePointPoint(Line2d li,Point2d pt1,Point2d pt2) {

            // max two solutions
            // solution created by Robert.P. 2013-02-07

            List<Circle2d> res = null;

            // transform problem so that we have a vertical line thru origo which simplifies stuff a lot
            Transform2d toorg = Transform2d.Translate(-li.X1, -li.Y1) * Transform2d.Rotate(-li.Angle + MathUtil.Deg90);
            Transform2d fromorg = toorg.Inversed;
            double i, j, k, l;
            toorg.Apply(pt1.X, pt1.Y, out i, out j, true);
            toorg.Apply(pt2.X, pt2.Y, out k, out l, true);

            
            double y2=2*k-2*i;
            double y1=4*i*l-4*j*k;
            double y0=-2*i*l*l-2*i*k*k+(2*j*j+2*i*i)*k;

            double[] ys=RealPolynomial.SolveQuadric(y2, y1, y0);
            if (ys == null)
                return null; //no solutions
            
            foreach(double y in ys) {
                double xx = (y * y - 2 * j * y + j * j + i * i) / (2 * i), yy=y; //TODO: what if vertical line
                double rad = Math.Abs(xx);

                fromorg.Apply(xx, yy, out xx, out yy, true);
                AddResult(ref res, xx, yy, rad);
            }


            return MakeResult(res);
        }

        public static Circle2d[] CircleCirclePoint(Circle2d cir1,Circle2d cir2,Point2d pnt) {
            //we do this the lazy way
            Circle2d c=new Circle2d(pnt,0.0);
            return CircleCircleCircle(cir1,cir2,c);
        }

        public static Circle2d[] CircleLinePoint(Circle2d cir, Line2d lin, Point2d pnt)
        {
            //do it easy for us
            Circle2d ptcir = new Circle2d(pnt, 0.0);
            return CircleCircleLine(cir, ptcir, lin);
        }
        

        private static Circle2d[] MakeResult(List<Circle2d> inlist)
        {
            if (inlist == null||inlist.Count<=0) 
                return null;
            return inlist.ToArray();
        }

        private static void AddResult(ref List<Circle2d> result, double xcenter, double ycenter, double rad)
        {
            if (MathUtil.IsZero(rad))
                return;

            if (result == null)
                result = new List<Circle2d>();

            foreach(Circle2d c in result) { // dont add equivalent circles
                if (MathUtil.Equals(c.X, xcenter) && MathUtil.Equals(c.Y, ycenter) && MathUtil.Equals(c.Radius, rad))
                    return; //same circle already exists
            }

            result.Add(new Circle2d(xcenter, ycenter, rad));
        }
    }
}
