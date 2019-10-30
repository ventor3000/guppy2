using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{
    public static class TanTanRadCircle
    {
        public static Circle2d[] PointPoint(Point2d p1, Point2d p2, double radius)
        {
            // compute the circles which intersect at p1,p2 with radius 'rad'
            double a = p1.Distance(p2);
            if (MathUtil.IsZero(a)) 
                return null;    //through one point
            
            double d = 4.0 * radius * radius - a * a;
            if (d < 0.0) return null; //not possible
            d = Math.Sqrt(d) * 0.5;

            //vector from midpoint of p1-p2 to center of circles
            //that is vector p2-p1, rotaded 90 degrees, normalized and multiplied by distance to circle center
            double vx = ((p2.Y - p1.Y) / a) * d;
            double vy = -((p2.X - p1.X) / a) * d;
            double midx = (p1.X + p2.X) * 0.5;
            double midy = (p1.Y + p2.Y) * 0.5;

            return new Circle2d[]{
                new Circle2d(midx+vx,midy+vy,radius),
                new Circle2d(midx-vx,midy-vy,radius)};
        }

        public static Circle2d[] LinePoint(Line2d lin, Point2d pnt, double radius)
        {

            //see 'a programmers geometry' page 36 for explanation

            double a, b, c;
            if(!lin.ToEquation(out a, out b, out c))
                return PointPoint(lin.MidPoint,pnt,radius);    //degenerate line
            
            c = c + a * pnt.X + b * pnt.Y; //new c for line equation corresponding to pnt beeing transformed to origo
            
            if (c < 0) { a = -a; b = -b; c = -c; } //make sure c is posetive


            double rt = c * (2.0 * radius - c);
            if (rt < -MathUtil.Epsilon) return null;  //point to far away from line, no solutions
            else
            {
                if (rt < 0.0) rt = 0.0; //protect from fuzzy span between -epsilon and zero

                double xj = -a * (c - radius);
                double yj = -b * (c - radius);

                if (MathUtil.Equals(rt, 0.0))
                { //one solution

                    if (MathUtil.IsZero(c)) //point is on line, we can still have two solutions
                    {
                        return new Circle2d[] { 
                            new Circle2d(pnt.X+xj, pnt.Y+yj, radius),
                            new Circle2d( pnt.X-xj,  pnt.Y-yj, radius),
                        };
                        
                    }

                    else
                    {
                        return new Circle2d[] { new Circle2d(xj + pnt.X, yj + pnt.Y, radius) };
                    }
                }
                else
                { //two solutions
                    rt = Math.Sqrt(rt);
                    return new Circle2d[] {
                    new Circle2d(xj+b*rt+pnt.X,yj-a*rt+pnt.Y,radius),
                    new Circle2d(xj-b*rt+pnt.X,yj+a*rt+pnt.Y,radius)
                };
                }
            }
        }

        public static Circle2d[] LineLine(Line2d lin1,Line2d lin2,double radius) {
            
            // See page 39 in 'a programmers geometry' for explanation

            double a1,b1,c1,a2,b2,c2;
            
            if(!lin1.ToEquation(out a1,out b1,out c1))
                return LinePoint(lin2,lin1.MidPoint,radius); //degenerate line 1 (if lin2 is degenerate as well, this is handled in LinePoint)
          
            if(!lin2.ToEquation(out a2,out b2,out c2))
                return LinePoint(lin1,lin2.MidPoint,radius); //degenerate line 2

            double denom=a2*b1-a1*b2;

            if(MathUtil.IsZero(denom))
                return null; //parallel lines

            Circle2d[] res=new Circle2d[4];

            for(int l=0;l<4;l++) {
                // note: this computation assumes that the line equation ax+by+c is normalized so that Math.Sqrt(a1 * a1 + b1 * b1) equals 1 which simplifies the computation
                int sign1,sign2;
                sign1= ((l&1)==0) ? 1:-1;
                sign2= ((l&2)==0) ? 1:-1;
                double x=(b2*(c1+radius*sign1)-b1*(c2+radius*sign2))/denom;
                double y=(a2*(c1+radius*sign1)-a1*(c2+radius*sign2))/-denom;
                res[l]=new Circle2d(x,y,radius);
            }

            return res.ToArray();           
        }

        public static Circle2d[] CirclePoint(Circle2d c, Point2d p, double radius)
        {
            // see 'a programmers geometry' page 41
            // modified to compute all potential 4 circles (with a loop) instead of outer or inner circles

            List<Circle2d> result = new List<Circle2d>();

            double xkj = c.X - p.X;
            double ykj = c.Y - p.Y;
            double kr = c.Radius; //we negate this in second turn of loop

            double sqsum = xkj * xkj + ykj * ykj;

            if (MathUtil.IsZero(sqsum)) return null; //k,j is coincident

            for (int q = 0; q < 2; q++)
            { //first turn computes outer circles, next inner circles
                double sqinv = 0.5 / sqsum;
                double radsum = (radius + radius + kr) * kr;
                double subexp = sqsum - radsum;
                double root = 4.0 * radius * radius * sqsum - subexp * subexp;
                subexp *= sqinv;
                if (root > -MathUtil.Epsilon)
                {

                    if (MathUtil.Equals(root, 0.0))
                    { //one solution possible
                        result.Add(new Circle2d(p.X + xkj * subexp, p.Y + ykj * subexp, radius));
                    }
                    else
                    { //two solutions possible
                        root = Math.Sqrt(root) * sqinv;
                        double xconst = p.X + xkj * subexp;
                        double yconst = p.Y + ykj * subexp;
                        double xvar = ykj * root;
                        double yvar = xkj * root;

                        result.Add(new Circle2d(xconst - xvar, yconst + yvar, radius));
                        result.Add(new Circle2d(xconst + xvar, yconst - yvar, radius));
                    }
                }

                kr = -kr;
                //radius = -radius;
            }

            if (result.Count <= 0)
                return null;
            return result.ToArray();
        }

        public static Circle2d[] CircleLine(Circle2d ci, Line2d li, double radius)
        {

            // see 'a programmers geometry' page 42 for explanation

            double a, b,c, rj, sa, sb, sc, xcons, ycons;
            List<Circle2d> result = new List<Circle2d>();

            if(!li.ToEquation(out a,out b,out c)) 
                return CirclePoint(ci,li.MidPoint,radius); //line was degenerate

            c = c + a * ci.Center.X + b * ci.Center.Y;    //transform line so that circle center is at origo

            for (int l = 0; l < 4; l++)
            {
                sa = ((l & 1) == 0) ? a:-a;
                sb = ((l & 1) == 0) ? b : -b;
                sc = ((l & 1) == 0) ? c : -c;
                rj = ((l&2)==0) ? ci.Radius:-ci.Radius;
                

                double root = (radius + rj) * (radius + rj) - (sc - radius) * (sc - radius);
                xcons = ci.Center.X - sa * (sc - radius);
                ycons = ci.Center.Y - sb * (sc - radius);

                if (root < -MathUtil.Epsilon) continue;  //circle center to far away
                else if (root < MathUtil.Epsilon)
                { //one solution possible
                    result.Add(new Circle2d(xcons,ycons,radius));
                }
                else
                {//tw solutions
                    root = Math.Sqrt(root);
                    result.Add(new Circle2d(xcons + sb * root, ycons - sa * root, radius));
                    result.Add(new Circle2d(xcons - sb * root, ycons + sa * root, radius));
                }
            }


            if (result.Count <= 0)
                return null;
            
            return result.ToArray();
        }

        public static Circle2d[] CircleCircle(Circle2d ci1,Circle2d ci2,double radius) {

            // Solution method by Robert.P. 2013-02-06

            List<Circle2d> res = null;

            double r1 = ci1.Radius;
            double r2 = ci2.Radius;
            double r3 = radius;
            double ang = ci1.Center.Angle(ci2.Center);
            double dist = ci1.Center.Distance(ci2.Center);

            if (dist < MathUtil.Epsilon) 
                return null; //concentric circles, no solution, or in special cases infinit number of solutions.

            for (int neg1 = 0; neg1 < 2; neg1++)
            {
                for (int neg2 = 0; neg2 < 2; neg2++)
                {
                    double sqr13 = MathUtil.Square(r1 + r3);
                    double sqr23 = MathUtil.Square(r2 + r3);
                    double dx = (sqr13 - sqr23 + dist * dist) / (2.0 * dist); //distance along c1-c2 of solution
                    double srt = sqr13 - dx * dx;
                    if (srt < 0.0)
                        continue; //to far away

                    double dy = Math.Sqrt(srt); //distance perpendicular to c1-c2 of soultion (negative or posetive)

                    if (res == null)
                        res = new List<Circle2d>();
                    Vector2d xv = Vector2d.FromAngleAndLength(ang, dx);
                    Vector2d yv = Vector2d.FromAngleAndLength(ang + MathUtil.Deg90, dy);

                    res.Add(new Circle2d(ci1.Center + xv + yv, radius));
                    res.Add(new Circle2d(ci1.Center + xv - yv, radius));

                    r2 = -r2;
                }
                r1 = -r1;
            }


            if (res == null)
                return null;

            return res.ToArray();         
        }

    }
}
