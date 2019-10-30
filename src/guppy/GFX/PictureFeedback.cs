using Guppy2.Calc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GFX
{
    public class PictureFeedback
    {
        class IntervalTreeInt:IEnumerable
        {
            class ITIntervalInt
            {
                public int min;
                public int max;
                public int mid;
                public object obj;
                public ITIntervalInt(object obj, int min, int max)
                {
                    this.min = Math.Min(min, max);
                    this.max = Math.Max(min, max);
                    this.obj = obj;
                    this.mid = (int)((min + max) * 0.5);
                }
            }
            class ITNodeInt
            {
                public int mid;
                public List<ITIntervalInt> left = new List<ITIntervalInt>();
                public List<ITIntervalInt> right = new List<ITIntervalInt>();
                public ITNodeInt lc = null;
                public ITNodeInt rc = null;
            }
            ITNodeInt root = null;
            List<ITIntervalInt> intervals = new List<ITIntervalInt>();
            List<object> objects = new List<object>();
            public int Count
            {
                get
                {
                    return objects.Count;
                }
            }
            public void Add(object obj, int min, int max)
            {
                intervals.Add(new ITIntervalInt(obj, min, max));
                objects.Add(obj);
                root = null;
            }
            public void Clear()
            {
                intervals.Clear();
                objects.Clear();
                root = null;
            }
            void Construct()
            {
                intervals.Sort((a, b) => { return a.mid.CompareTo(b.mid); });
                root = ConstructRecursive(intervals);
            }
            private ITNodeInt ConstructRecursive(List<ITIntervalInt> L)
            {
                if (L.Count <= 0)
                    return null;
                ITNodeInt n = new ITNodeInt();
                n.mid = Median(L);
                List<ITIntervalInt> left = new List<ITIntervalInt>();
                List<ITIntervalInt> right = new List<ITIntervalInt>();
                foreach (ITIntervalInt i in L)
                {
                    if (i.max < n.mid)
                        left.Add(i);
                    else if (i.min > n.mid)
                        right.Add(i);
                    else
                    {
                        n.left.Add(i);
                        n.right.Add(i);
                    }
                }
                n.left.Sort((a, b) => { return a.min.CompareTo(b.min); });
                n.right.Sort((a, b) => { return b.max.CompareTo(a.max); });
                n.lc = ConstructRecursive(left);
                n.rc = ConstructRecursive(right);
                return n;
            }
            int Median(List<ITIntervalInt> l)
            {
                int n = l.Count;
                if (n == 1)
                    return l[0].mid;
                int id = (int)(n * 0.5);
                if (n % 2 == 0)
                    return l[id].mid;
                return (int)((l[id].mid + l[id - 1].mid) * 0.5);
            }
            public List<object> Find(int val)
            {
                if (root == null)
                    Construct();
                List<object> res = new List<object>();
                if (root != null)
                    FindRecursive(root, val, ref res);
                return res;
            }
            private void FindRecursive(ITNodeInt n, int val, ref List<object> res)
            {
                if (n == null)
                    return;
                if (val <= n.mid)
                {
                    foreach (ITIntervalInt i in n.left)
                    {
                        if (i.min > val)
                            break;
                        if (i.max > val || i.min == i.max)
                            res.Add(i.obj);
                    }
                    FindRecursive(n.lc, val, ref res);
                }
                else
                {
                    foreach (ITIntervalInt i in n.right)
                    {
                        if (i.max < val)
                            break;
                        res.Add(i.obj);
                    }
                    FindRecursive(n.rc, val, ref res);
                }
            }
            public List<object> Find(int val1, int val2, Func<object, bool> filter = null)
            {
                if (val1 > val2)
                {
                    int tmp = val1;
                    val1 = val2;
                    val2 = tmp;
                }
                if (root == null)
                    Construct();
                List<object> res = new List<object>();
                if (root != null)
                    FindRecursive(root, val1, val2, ref res, filter);
                return res;
            }
            private void FindRecursive(ITNodeInt n, int val1, int val2, ref List<object> res, Func<object, bool> filter)
            {
                if (n == null)
                    return;
                if (val2 <= n.mid)
                {
                    foreach (ITIntervalInt i in n.left)
                    {
                        if (i.min > val2)
                            break;
                        if (filter == null || filter(i.obj))
                            res.Add(i.obj);
                    }
                    FindRecursive(n.lc, val1, val2, ref res, filter);
                }
                else if (val1 >= n.mid)
                {
                    foreach (ITIntervalInt i in n.right)
                    {
                        if (i.max < val1)
                            break;
                        if (filter == null || filter(i.obj))
                            res.Add(i.obj);
                    }
                    FindRecursive(n.rc, val1, val2, ref res, filter);
                }
                else
                {
                    foreach (ITIntervalInt i in n.left)
                    {
                        if (i.min > val2)
                            break;
                        if (filter == null || filter(i.obj))
                            res.Add(i.obj);
                    }
                    FindRecursive(n.lc, val1, n.mid, ref res, filter);
                    FindRecursive(n.rc, n.mid, val2, ref res, filter);
                }
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return objects.GetEnumerator();
            }
        }
        class IntLine
        {
            public int x1;
            public int y1;
            public int x2;
            public int y2;
            public object obj;
            public IntLine(int x1, int y1, int x2, int y2, object obj)
            {
                this.x1 = x1;
                this.x2 = x2;
                this.y1 = y1;
                this.y2 = y2;
                this.obj = obj;
            }
        }



        IntervalTreeInt tx = new IntervalTreeInt();
        Dictionary<object, int> objects = new Dictionary<object, int>();
        public readonly int Id;
        public int ObjectCount
        {
            get
            {
                return objects.Count;
            }
        }
        public int TreeCount
        {
            get
            {
                return tx.Count;
            }
        }
        public int Width { get; set; }
        public int Height { get; set; }
        public PictureFeedback(int width, int height, int id)
        {
            Id = id;
            Width = width;
            Height = height;
        }
        public void Clear()
        {
            tx.Clear();
            objects.Clear();
        }

        /// <summary>
        /// Adds an object to the picture
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="obj"></param>
        public void Add(int x1, int y1, int x2, int y2, object obj)
        {
            if (!objects.ContainsKey(obj))
                objects[obj] = 0;
            objects[obj] += 1;
            //if (LineVisible(x1, y1, x2, y2, 0, 0, Width - 1, Height - 1))
                tx.Add(new IntLine(x1, y1, x2, y2, obj), Math.Min(x1, x2), Math.Max(x1, x2));
        }

        public List<T> BoxCrossing<T>(Rectangle2i rect)
        {
            return BoxCrossing<T>(rect.XMin, rect.YMin, rect.XMax, rect.YMax);
        }

        public List<T> BoxCrossing<T>(int x1, int y1, int x2, int y2) 
        {
            Dictionary<object, bool> r = new Dictionary<object, bool>();
            List<T> result = new List<T>();
            List<object> res = tx.Find(x1, x2, (o) =>
            {
                IntLine l = o as IntLine;
                if (l.y1 < y1 && l.y2 < y1)
                    return false;
                if (l.y1 > y2 && l.y2 > y2)
                    return false;
                if (r.ContainsKey(l.obj))
                    return false;
                if (!LineVisible(l.x1, l.y1, l.x2, l.y2, x1, y1, x2, y2))
                    return false;
                r[l.obj] = true;
                
                if(l.obj is T)
                    result.Add((T)l.obj);
                return true;
            });
            return result;
        }

        public List<T> BoxWindow<T>(Rectangle2i rect)
        {
            return BoxWindow<T>(rect.XMin, rect.YMin, rect.XMax, rect.YMax);
        }

        public List<T> BoxWindow<T>(int x1, int y1, int x2, int y2)
        {
            Dictionary<object, int> r = new Dictionary<object, int>();
            List<T> result = new List<T>();
            List<object> res = tx.Find(x1, x2, (o) =>
            {
                IntLine l = o as IntLine;
                if (l.x1 < x1 || l.x1 > x2 || l.x2 < x1 || l.x2 > x2 || l.y1 < y1 || l.y1 > y2 || l.y2 < y1 || l.y2 > y2)
                    return false;
                if (!r.ContainsKey(l.obj))
                    r[l.obj] = 0;
                r[l.obj] += 1;
                if (r[l.obj] == objects[l.obj] && l.obj is T)
                    result.Add((T)l.obj);
                return true;
            });
            return result;
        }

        public List<T> PolyCrossing<T>(List<Point2i> pnts)
        {
            Dictionary<object, bool> r = new Dictionary<object, bool>();
            List<T> result = new List<T>();

            // Bounding box
            int x1 = int.MaxValue;
            int y1 = int.MaxValue;
            int x2 = int.MinValue;
            int y2 = int.MinValue;

            // First create an Y-interval tree for fast inside/intersection checking
            IntervalTreeInt ins = new IntervalTreeInt();
            int cnt = pnts.Count;
            for (int i = 0; i < cnt; i++)
            {
                IntLine l = new IntLine(pnts[i].X, pnts[i].Y, pnts[(i + 1) % cnt].X, pnts[(i + 1) % cnt].Y, null);
                ins.Add(l, Math.Min(l.y1, l.y2), Math.Max(l.y1, l.y2));
                // Calc bbox
                if (pnts[i].X < x1)
                    x1 = pnts[i].X;
                if (pnts[i].X > x2)
                    x2 = pnts[i].X;
                if (pnts[i].Y < y1)
                    y1 = pnts[i].Y;
                if (pnts[i].Y > y2)
                    y2 = pnts[i].Y;
            }

            List<object> res = tx.Find(x1, x2, (o) =>
            {
                IntLine l = o as IntLine;
                if (l.y1 < y1 && l.y2 < y1)
                    return false;
                if (l.y1 > y2 && l.y2 > y2)
                    return false;
                if (r.ContainsKey(l.obj))
                    return false;
                // Collect all candidates from poly
                List<object> candidates = ins.Find(l.y1, l.y2, null);
                if (Inside(l.x1, l.y1, candidates) || Inside(l.x2, l.y2, candidates) || Intersect(l, candidates))
                {
                    r[l.obj] = true;
                    if(l.obj is T)
                        result.Add((T)l.obj);
                    return true;
                }
                return false;
            });
            return result;
        }

        public List<T> PolyWindow<T>(List<Point2i> pnts)
        {
            Dictionary<object, int> r = new Dictionary<object, int>();
            List<T> result = new List<T>();

            // Bounding box
            int x1 = int.MaxValue;
            int y1 = int.MaxValue;
            int x2 = int.MinValue;
            int y2 = int.MinValue;

            // First create an Y-interval tree for fast inside/intersection checking
            IntervalTreeInt ins = new IntervalTreeInt();
            int cnt = pnts.Count;
            for (int i = 0; i < cnt; i++)
            {
                IntLine l = new IntLine(pnts[i].X, pnts[i].Y, pnts[(i + 1) % cnt].X, pnts[(i + 1) % cnt].Y, null);
                ins.Add(l, Math.Min(l.y1, l.y2), Math.Max(l.y1, l.y2));
                // Calc bbox
                if (pnts[i].X < x1)
                    x1 = pnts[i].X;
                if (pnts[i].X > x2)
                    x2 = pnts[i].X;
                if (pnts[i].Y < y1)
                    y1 = pnts[i].Y;
                if (pnts[i].Y > y2)
                    y2 = pnts[i].Y;
            }

            List<object> res = tx.Find(x1, x2, (o) =>
            {
                IntLine l = o as IntLine;
                if (l.y1 < y1 && l.y2 < y1)
                    return false;
                if (l.y1 > y2 && l.y2 > y2)
                    return false;
                // Collect all candidates from poly
                List<object> candidates = ins.Find(l.y1, l.y2, null);
                if (Inside(l.x1, l.y1, candidates) && Inside(l.x2, l.y2, candidates) && !Intersect(l, candidates))
                {
                    if (!r.ContainsKey(l.obj))
                        r[l.obj] = 0;
                    r[l.obj] += 1;
                    if (r[l.obj] == objects[l.obj] && l.obj is T)
                        result.Add((T)l.obj);
                    return true;
                }
                return false;
            });
            return result;
        }

        public List<T> Fence<T>(List<Point2i> pnts, Func<int,int,int,int,int,int> plot=null)             
        {
            Dictionary<object, bool> r = new Dictionary<object, bool>();
            List<T> result = new List<T>();
            int cnt = pnts.Count;
            for (int i = 0; i < cnt - 1; i++)
            {
                IntLine seg = new IntLine(pnts[i].X, pnts[i].Y, pnts[(i + 1) % cnt].X, pnts[(i + 1) % cnt].Y, null);
                if (plot != null)
                    plot(seg.x1, seg.y1, seg.x2, seg.y2, 3);
                int x1 = Math.Min(seg.x1, seg.x2);
                int y1 = Math.Min(seg.y1, seg.y2);
                int x2 = Math.Max(seg.x1, seg.x2);
                int y2 = Math.Max(seg.y1, seg.y2);
                List<object> res = tx.Find(seg.x1, seg.x2, (o) =>
                {
                    IntLine l = o as IntLine;
                    if (Outside(l, x1,y1,x2,y2))
                        return false;
                    if (r.ContainsKey(l.obj))
                        return false;
                    if (Intersect(l, seg))
                    {
                        r[l.obj] = true;
                        return true;
                    }
                    return false;
                });
                if (res.Count <= 0)
                    continue;
                else if (res.Count == 1)
                {
                    object o = ((IntLine)res[0]).obj;
                    if(o is T)
                        result.Add((T)o);
                }
                else
                {
                    // Loop res and calc proper intersections and sort then add
                    List<KeyValuePair<IntLine, double>> ints = new List<KeyValuePair<IntLine, double>>();
                    foreach (IntLine l in res)
                    {
                        double s = SegmentSegment(seg, l);
                        ints.Add(new KeyValuePair<IntLine, double>(l, s));
                    }
                    ints.Sort((a, b) => { return a.Value.CompareTo(b.Value); });
                    foreach (var l in ints)
                    {
                        if (l.Key.obj is T)
                            result.Add((T)(l.Key.obj));
                    }
                }
            }
            if (plot!=null)
            {
                foreach (IntLine l in tx)
                    plot(l.x1, l.y1, l.x2, l.y2, r.ContainsKey(l.obj)?5:1);
            }
            return result;
        }
        public void Draw(Painter p)
        {
            foreach (IntLine l in tx)
                p.DrawLine(l.x1, l.y1, l.x2, l.y2);
        }
        bool Outside(IntLine l, int x1, int y1, int x2, int y2)
        {
            if (l.x1 < x1 && l.x2 < x1)
                return true;
            if (l.y1 < y1 && l.y2 < y1)
                return true;
            if (l.x1 > x2 && l.x2 > x2)
                return true;
            if (l.y1 > y2 && l.y2 > y2)
                return true;
            return false;
        }
        bool Inside(int x, int y, List<object> lines)
        {
            bool c = false;
            foreach (IntLine l in lines)
            {
                if (l.y1 == l.y2)
                    continue;
                bool up2 = l.y1 < l.y2 || (l.y1 == l.y2 && l.x1 < l.x2);
                bool side = SideOf(l.x1, l.y1, l.x2, l.y2, x, y) > 0;
                if (side == up2)
                    c = !c;
            }
            return c;
        }
        bool Intersect(IntLine l1, IntLine l2)
        {
            return Intersect(l1.x1, l1.y1, l1.x2, l1.y2, l2.x1, l2.y1, l2.x2, l2.y2);
        }
        bool Intersect(IntLine l, List<object> lines)
        {
            foreach (IntLine l2 in lines)
            {
                if (Intersect(l.x1, l.y1, l.x2, l.y2, l2.x1, l2.y1, l2.x2, l2.y2))
                    return true;
            }
            return false;
        }
        bool Intersect(int l1x1, int l1y1, int l1x2, int l1y2, int l2x1, int l2y1, int l2x2, int l2y2)
        {
            int a1 = SideOf(l1x1, l1y1, l1x2, l1y2, l2x1, l2y1);
            int a2 = SideOf(l1x1, l1y1, l1x2, l1y2, l2x2, l2y2);
            if ((a1 < 0 && a2 < 0) || (a1 > 0 && a2 > 0))
                return false;
            a1 = SideOf(l2x1, l2y1, l2x2, l2y2, l1x1, l1y1);
            a2 = SideOf(l2x1, l2y1, l2x2, l2y2, l1x2, l1y2);
            if ((a1 < 0 && a2 < 0) || (a1 > 0 && a2 > 0))
                return false;
            return true;

            //return CCW(l1x1, l1y1, l2x1, l2y1, l2x2, l2y2) != CCW(l1x2, l1y2, l2x1, l2y1, l2x2, l2y2) && CCW(l1x1, l1y1, l1x2, l1y2, l2x1, l2y1) != CCW(l1x1, l1y1, l1x2, l1y2, l2x2, l2y2);
        }
        /// <summary>
        /// Returns parameter on l1
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        /// <returns></returns>
        double SegmentSegment(IntLine l1, IntLine l2)
        {
            double denom = l1.x1 * (l2.y2 - l2.y1) + l1.x2 * (l2.y1 - l2.y2) + l2.x2 * (l1.y2 - l1.y1) + l2.x1 * (l1.y1 - l1.y2);
            if (denom != 0.0)
                return (l1.x1 * (l2.y2 - l2.y1) + l2.x1 * (l1.y1 - l2.y2) + l2.x2 * (l2.y1 - l1.y1)) / denom;

            //handle parallel lines
            double dist1 = MathUtil.Hypot(l1.x1 - l2.x1, l1.y1 - l2.y1);
            double dist2 = MathUtil.Hypot(l1.x1 - l2.x2, l1.y1 - l2.y2);
            double l = MathUtil.Hypot(l1.x1 - l1.x2, l1.y1 - l1.y2);
            if (l < 1e-3)
                return 0.0;
            if(dist1<dist2) 
                return dist1/l;
            else
                return dist1/l;
            
        }

        /// <summary>
        /// Negative=RIGHT
        /// Positive=LEFT
        /// 0=ON
        /// </summary>
        /// <param name="p0x"></param>
        /// <param name="p0y"></param>
        /// <param name="p1x"></param>
        /// <param name="p1y"></param>
        /// <param name="pointx"></param>
        /// <param name="pointy"></param>
        /// <returns></returns>
        int SideOf(int p0x, int p0y, int p1x, int p1y, int pointx, int pointy)
        {
            return (p0x - p1x) * (p0y - pointy) - (p0y - p1y) * (p0x - pointx);
        }

        bool LineVisible(int x1, int y1, int x2, int y2, int left, int bottom, int right, int top)
        {
            // this function clips the sent line using the defined clipping
            // region using the cohen-sutherland clipping algorithm
            // returns false if the line is totally invisible, otherwise true

            //return Clipper.LineVisible(x1, y1, x2, y2, clip_min_x, clip_min_y, clip_max_x, clip_max_y);
            // internal clipping codes
            const int clipCodeN = 0x0008;
            const int clipCodeS = 0x0004;
            const int clipCodeE = 0x0002;
            const int clipCodeW = 0x0001;
            int p1Code = 0, p2Code = 0;

            // determine codes for p1 and p2
            if (y1 < bottom)
                p1Code |= clipCodeN;
            else if (y1 > top)
                p1Code |= clipCodeS;

            if (x1 < left)
                p1Code |= clipCodeW;
            else if (x1 > right)
                p1Code |= clipCodeE;

            if (y2 < bottom)
                p2Code |= clipCodeN;
            else if (y2 > top)
                p2Code |= clipCodeS;

            if (x2 < left)
                p2Code |= clipCodeW;
            else if (x2 > right)
                p2Code |= clipCodeE;

            // try and trivially reject
            if ((p1Code & p2Code) != 0) //same side, must be totally invisible
                return false;

            // test for totally visible, if so leave points untouched
            if (p1Code == 0 && p2Code == 0) //both points inside must be fully visible
                return true;

            int flag = 0;
            int dx = x1 - x2;
            int dy = y1 - y2;
            if (dx * (y1 - bottom) - dy * (x1 - left) <= 0)
                flag |= 1;
            if (dx * (y1 - bottom) - dy * (x1 - right) <= 0)
                flag |= 2;
            if (flag != 0 && flag != 3)
                return true;
            if (dx * (y1 - top) - dy * (x1 - right) <= 0)
                flag |= 4;
            if (flag != 0 && flag != 7)
                return true;
            if (dx * (y1 - top) - dy * (x1 - left) <= 0)
                flag |= 8;
            return flag != 0 && flag != 15;
        }

    }
}
