using Guppy2.Calc;
using Guppy2.Calc.Geom2d;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace Guppy2.GFX
{

    public class GFXPathCache2
    {
        public object CacheObject1 = null;
        public object CacheObject2 = null;
        public Box2d BoundingBox = null;
    }

    public class GFXPathMoveTo
    {
        public readonly double X;
        public readonly double Y;

        public GFXPathMoveTo(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        internal virtual void Flatten(ref double penx, ref double peny, double tol, FlattenCallback lcb)
        {
            lcb(X, Y, true);
            penx = X;
            peny = Y;
        }

        internal virtual void TransformFlatten(ref double penx, ref double peny, double tol, Transform2d t, FlattenCallback lcb)
        {
            penx = X;
            peny = Y;
            double tx, ty;
            t.Apply(penx, peny, out tx, out ty, true);
            lcb(tx, ty, true);
        }

        
    }

    public class GFXPathLineTo : GFXPathMoveTo
    {
        public GFXPathLineTo(double x, double y)
            : base(x, y)
        {

        }

        internal override void Flatten(ref double penx, ref double peny, double tol, FlattenCallback lcb)
        {
            lcb(X, Y, false);
            penx = X;
            peny = Y;
        }

        internal override void TransformFlatten(ref double penx, ref double peny, double tol, Transform2d t, FlattenCallback lcb)
        {
            penx = X;
            peny = Y;
            double tx, ty;
            t.Apply(penx, peny, out tx, out ty, true);
            lcb(tx, ty, false);
        }
    }

    public class GFXPathCloseSubPath : GFXPathMoveTo
    {
        public GFXPathCloseSubPath(double x, double y)
            : base(x, y)
        {
            //x,y is assummed to be the last move to
        }
    }

    public class GFXPathArcTo : GFXPathMoveTo
    {
        public readonly double Bulge;

        public GFXPathArcTo(double x, double y, double bulge)
            : base(x, y)
        {
            this.Bulge = bulge;
        }

        internal override void Flatten(ref double penx, ref double peny, double tol, FlattenCallback lcb)
        {
            GeomUtil.FlattenArc(penx, peny, X, Y, Bulge, false, tol, lcb);
            penx = X;
            peny = Y;
        }

        internal override void TransformFlatten(ref double penx, ref double peny, double tol, Transform2d t, FlattenCallback lcb)
        {
            GeomUtil.TransformFlattenArc(penx, peny, X, Y, Bulge, false, tol, t, lcb);
            penx = X;
            peny = Y;
        }
    }

    public class GFXPathBezierTo : GFXPathMoveTo
    {
        public readonly double XC1;
        public readonly double YC1;
        public readonly double XC2;
        public readonly double YC2;

        public GFXPathBezierTo(double ctl1x, double ctl1y, double ctl2x, double ctl2y, double x, double y)
            : base(x, y)
        {
            this.XC1 = ctl1x;
            this.YC1 = ctl1y;
            this.XC2 = ctl2x;
            this.YC2 = ctl2y;
        }

        internal override void Flatten(ref double penx, ref double peny, double tol, FlattenCallback lcb)
        {
            GeomUtil.FlattenBezier(penx, peny, XC1, YC1, XC2, YC2, X, Y, false, tol, lcb);
            penx = X;
            peny = Y;
        }

        internal override void TransformFlatten(ref double penx, ref double peny, double tol, Transform2d t, FlattenCallback lcb)
        {

            double xs, ys, c1x, c1y, c2x, c2y, xe, ye;
            t.Apply(penx, peny, out xs, out ys, true);
            t.Apply(XC1, YC1, out c1x, out c1y, true);
            t.Apply(XC2, YC2, out c2x, out c2y, true);
            t.Apply(X, Y, out xe, out ye, true);
            GeomUtil.FlattenBezier(xs, ys, c1x, c1y, c2x, c2y, xe, ye, false, tol, lcb);
            penx = X;
            peny = Y;
        }
    }




    public class GFXPath
    {

        public List<GFXPathMoveTo> PathPoints = new List<GFXPathMoveTo>();  //TODO: make private
        GFXPathCache2 cache;

        double subpath_start_x = 0.0;
        double subpath_start_y = 0.0;

        public void MoveTo(double x, double y)
        {
            PathPoints.Add(new GFXPathMoveTo(x, y));
            subpath_start_x = x;
            subpath_start_y = y;
            cache = null;
        }

        public void LineTo(double x, double y)
        {
            PathPoints.Add(new GFXPathLineTo(x, y));
            cache = null;
        }

        public void ArcTo(double x, double y, double bulge)
        {
            cache = null;

            if (Math.Abs(bulge) < MathUtil.Epsilon)
            {
                LineTo(x, y);
                return;
            }

            PathPoints.Add(new GFXPathArcTo(x, y, bulge));

        }

        public void BezierTo(double xctl1, double yctl1, double xctl2, double yctl2, double xend, double yend)
        {
            PathPoints.Add(new GFXPathBezierTo(xctl1, yctl1, xctl2, yctl2, xend, yend));
            cache = null;
        }

        public void CloseSubPath()
        {
            PathPoints.Add(new GFXPathCloseSubPath(subpath_start_x, subpath_start_y));
            cache = null;
        }


        public void AppendPath(GFXPath path)
        {
            if (path != null && path.PathPoints.Count != 0)
            {
                PathPoints.AddRange(path.PathPoints);
                cache = null;
            }
        }

        public void Flatten(double tol, FlattenCallback lcb)
        {
            double penx = 0.0, peny = 0.0;

            foreach (GFXPathMoveTo node in PathPoints)
                node.Flatten(ref penx, ref peny, tol, lcb);
        }

        public void TransformFlatten(double tol, Transform2d t, FlattenCallback lcb)
        {
            double penx = 0.0, peny = 0.0;
            foreach (GFXPathMoveTo node in PathPoints)
                node.TransformFlatten(ref penx, ref peny, tol, t, lcb);
        }

        public GFXPathCache2 Cache
        {
            get
            {
                if (cache == null)
                    cache = new GFXPathCache2();
                return cache;
            }
        }


        public object CacheObject1
        {
            get
            {
                if (cache == null) return null;
                return cache.CacheObject1;
            }
            set
            {
                var c = Cache;
                c.CacheObject1 = value;
            }
        }

        public object CacheObject2
        {
            get
            {
                if (cache == null) return null;
                return cache.CacheObject2;
            }
            set
            {
                var c = Cache;
                c.CacheObject2 = value;
            }
        }


        /// <summary>
        /// Computes the extents of the path. This extent is cached internally in the path, so if
        /// it is queried more than once without the path beeing changed, 
        /// it is very quick after the first time.
        /// </summary>
        public Box2d Extents
        {
            get
            {
                if (cache != null && cache.BoundingBox != null)
                    return cache.BoundingBox;

                Box2d res = new Box2d();

                double penx = 0.0, peny = 0.0;

                foreach (GFXPathMoveTo node in PathPoints)
                {
                    if (node is GFXPathLineTo)
                        res.Append(node.X, node.Y, penx, peny);
                    else if (node is GFXPathArcTo)
                        res.Append(GeomUtil.GetArcExtents(penx, peny, node.X, node.Y, ((GFXPathArcTo)node).Bulge));
                    else if (node is GFXPathBezierTo)
                    {
                        GFXPathBezierTo bn = (GFXPathBezierTo)node;
                        res.Append(GeomUtil.GetBezierExtents(penx, peny, bn.XC1, bn.YC1, bn.XC2, bn.YC2, bn.X, bn.Y));
                    }


                    penx = node.X;
                    peny = node.Y;
                }


                Cache.BoundingBox = res;
                return res;
            }
        }


        /// <summary>
        /// Computes the extents of the path, if the path had been transformed with
        /// the given transform.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Box2d GetTransformedExtents(Transform2d t)
        {
            Box2d res = new Box2d();

            double penx = 0.0, peny = 0.0;
            double x, y;

            foreach (GFXPathMoveTo node in PathPoints)
            {
                if (node is GFXPathLineTo)
                {
                    t.Apply(node.X, node.Y, out x, out y,true);
                    res.Append(x, y);
                    t.Apply(penx, peny, out x, out y,true);
                    res.Append(x,y);
                }
                else if (node is GFXPathArcTo)
                {

                    res.Append(GeomUtil.GetTransformedArcExtents(penx, peny, node.X, node.Y, ((GFXPathArcTo)node).Bulge, t));
                    
                }
                else if (node is GFXPathBezierTo)
                {
                    GFXPathBezierTo bn = (GFXPathBezierTo)node;
                    res.Append(GeomUtil.GetTransformedBezierExtents(penx, peny, bn.XC1, bn.YC1, bn.XC2, bn.YC2, bn.X, bn.Y,t));
                }


                penx = node.X;
                peny = node.Y;
            }

            return res;
        }


        /// <summary>
        /// Checks if this path, after a given transformation will have a bounding box smaller than a given size. THis can be done quickly since the
        /// check can be terminated as soon as it finds a segment large enough.
        /// </summary>
        public bool IsTransformedSmallerThan(Transform2d tr, double maxwidth, double maxheight)
        {
            double penx = 0.0, peny = 0.0;
            Box2d ext=new Box2d();

            foreach (GFXPathMoveTo mt in PathPoints)
            {
                if (mt is GFXPathBezierTo)
                {
                    var bez = mt as GFXPathBezierTo;
                    Box2d bext=GeomUtil.GetTransformedBezierExtents(penx, peny, bez.XC1, bez.YC1, bez.XC2, bez.YC2, bez.X, bez.Y, tr);
                    ext.Append(bext);
                }
                else if (mt is GFXPathArcTo)
                {
                    var arc = mt as GFXPathArcTo;
                    Box2d aext = GeomUtil.GetTransformedArcExtents(penx, peny, arc.X, arc.Y, arc.Bulge, tr);
                    ext.Append(aext);
                }
                else if (mt is GFXPathLineTo)
                {
                    double x1, y1, x2, y2;
                    tr.Apply(penx, peny, out x1, out y1, true);
                    tr.Apply(mt.X, mt.Y, out x2, out y2, true);
                    ext.Append(x1,y1,x2,y2);
                }

                // closepath and moveto needs no special handling...
                if (!ext.Empty &&(ext.Width > maxwidth || ext.Height > maxheight))
                    return false;
                
                penx = mt.X;
                peny = mt.Y;
            }


            return true;
        }

        public void EnumerateSegments(LineCallback lc, ArcCallback ac, BezierCallback bc)
        {
            double penx = 0.0, peny = 0.0;

            foreach (GFXPathMoveTo mt in PathPoints)
            {
                if (mt is GFXPathBezierTo)
                {
                    var bez = mt as GFXPathBezierTo;
                    bc(penx, peny, bez.XC1, bez.YC1, bez.XC2, bez.YC2, bez.X, bez.Y);
                }
                else if (mt is GFXPathArcTo)
                {
                    var arc = mt as GFXPathArcTo;
                    ac(penx, peny, arc.X, arc.Y, arc.Bulge);
                }
                else if (mt is GFXPathLineTo)
                {
                    lc(penx, peny, mt.X, mt.Y);
                }

                penx = mt.X;
                peny = mt.Y;
            }
        }

        public GFXPath TransformCopy(Transform2d tr)
        {
            GFXPath res = new GFXPath();
            double x1,y1,x2,y2,x3,y3,b;
            bool mirror = tr.Determinant < 0.0;

            foreach (GFXPathMoveTo mt in PathPoints)
            {
                if (mt is GFXPathBezierTo)
                {
                    var bez = mt as GFXPathBezierTo;
                    tr.Apply(bez.XC1,bez.YC1,out x1,out y1,true);
                    tr.Apply(bez.XC2,bez.YC2,out x2,out y2,true);
                    tr.Apply(bez.X,bez.Y,out x3,out y3,true);
                    res.PathPoints.Add(new GFXPathBezierTo(x1, y1, x2, y2, x3, y3));
                    
                }
                else if (mt is GFXPathArcTo)
                {
                    var arc = mt as GFXPathArcTo;
                    tr.Apply(arc.X, arc.Y, out x1, out y1, true);
                    b = mirror ? -arc.Bulge : arc.Bulge;
                    res.PathPoints.Add(new GFXPathArcTo(x1,y1,b));
                }
                else if (mt is GFXPathLineTo)
                {
                    var lin = mt as GFXPathLineTo;
                    tr.Apply(lin.X, lin.Y, out x1, out y1, true);
                    res.PathPoints.Add(new GFXPathLineTo(x1,y1));
                }
                else if(mt is GFXPathCloseSubPath) {
                    var clp = mt as GFXPathCloseSubPath;
                    tr.Apply(clp.X, clp.Y, out x1, out y1, true);
                    res.PathPoints.Add(new GFXPathCloseSubPath(x1,y1));
                }
                else { //move to
                    tr.Apply(mt.X, mt.Y, out x1, out y1, true);
                    res.PathPoints.Add(new GFXPathMoveTo(x1,y1));
                }
            }

            return res;
        }

    }
}
