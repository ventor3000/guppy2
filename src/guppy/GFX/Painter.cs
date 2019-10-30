using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Reflection;
using Guppy2.GFX.Util;
using Guppy2.Calc.Geom2d;
using Guppy2.Calc;
using Guppy2.AppUtils;

namespace Guppy2.GFX
{
    public enum LineJoin
    {
        Miter,
        Bevel,
        Round
    }

    public enum EndCap
    {
        Flat,
        Square,
        Round
    }

    
    public enum FillMode
    {
        EvenOdd,
        NonZero
    }

    public enum FillStyle
    {
        Solid,  //default
        Hatch,
        Pattern
    }
    
    public enum Hatch
    {
        Horizontal,             // -
        Vertical,               // |
        ForwardDiagonal,        // \
        BackwardDiagonal,       // /
        Cross,                  // +
        DiagonalCross,          // X
        Checkers                
    }


    public enum LineStyle
    {
        Continuous,
        Dash,
        Dot,
        DashDot,
        DashDotDot,
        Custom
    }

    [Flags]
    public enum MarkType
    {
        None=0,
        LargeCross=1,
        Cross=2,
        DiagonalCross=4,
        Slit=8,
        Pixel=16,
        Circle=32,
        Triangle=64,
        Diamond=128,
        Rectangle=256,
        FilledRectangle=512,
        Roof=1024,
        Floor=2048,
        Perpendicular=4096,
        Ellipsis=8192
    }


    public enum TextAlign
    {

        //y top=1 center=2 base=4 bottom=8
        //x left=16 center=32 right=64
        TopLeft = TextAlignAtom.TEXT_TOP | TextAlignAtom.TEXT_LEFT,
        TopCenter = TextAlignAtom.TEXT_TOP | TextAlignAtom.TEXT_CENTER_X,
        TopRight = TextAlignAtom.TEXT_TOP | TextAlignAtom.TEXT_RIGHT,
        CenterLeft = TextAlignAtom.TEXT_CENTER_Y | TextAlignAtom.TEXT_LEFT,
        Center = TextAlignAtom.TEXT_CENTER_Y | TextAlignAtom.TEXT_CENTER_X,
        CenterRight = TextAlignAtom.TEXT_CENTER_Y | TextAlignAtom.TEXT_RIGHT,
        BaseLeft = TextAlignAtom.TEXT_BASE | TextAlignAtom.TEXT_LEFT,
        BaseCenter = TextAlignAtom.TEXT_BASE | TextAlignAtom.TEXT_CENTER_X,
        BaseRight = TextAlignAtom.TEXT_BASE | TextAlignAtom.TEXT_RIGHT,
        BottomLeft = TextAlignAtom.TEXT_BOTTOM | TextAlignAtom.TEXT_LEFT,
        BottomCenter = TextAlignAtom.TEXT_BOTTOM | TextAlignAtom.TEXT_CENTER_X,
        BottomRight = TextAlignAtom.TEXT_BOTTOM | TextAlignAtom.TEXT_RIGHT
    }

    internal static class TextAlignAtom
    {
        internal const TextAlign TEXT_TOP = (TextAlign)1;
        internal const TextAlign TEXT_CENTER_Y = (TextAlign)2;
        internal const TextAlign TEXT_BASE = (TextAlign)4;
        internal const TextAlign TEXT_BOTTOM = (TextAlign)8;
        internal const TextAlign TEXT_LEFT = (TextAlign)16;
        internal const TextAlign TEXT_CENTER_X = (TextAlign)32;
        internal const TextAlign TEXT_RIGHT = (TextAlign)64;
    }


    public abstract class Painter : IDisposable
    {

        private static Dictionary<string, GFNFont> loadedfonts=new Dictionary<string,GFNFont>(StringComparer.InvariantCultureIgnoreCase);    //font lookup table, all loaded general fonts so far

        #region ATTRIBUTES
        
        private int color = 0;
        public virtual int Color { get { return color; } set { color = value; } }

        public virtual bool AntiGrain { get; set; }

        private Hatch hatch = Hatch.Cross;
        public virtual Hatch Hatch { get { return hatch; } set { hatch = value; FillStyle = FillStyle.Hatch; } }

        private FillStyle fillstyle = FillStyle.Solid;
        public virtual FillStyle FillStyle { get { return fillstyle; } set { fillstyle = value; } }
        
        public virtual double LineWidth { get; set; }  //default: 0.0, thinnest possible on device
        
        private LineStyle linestyle = LineStyle.Continuous;
        public virtual LineStyle LineStyle { get { return linestyle; } set { linestyle = value; } }

        internal static readonly double[] DefaultLineStyleDashes=new double[] { 4,4 }; //stock object
        private double[] linestyledashes = DefaultLineStyleDashes;
        public virtual double[] LineStyleDashes
        {
            get { return linestyledashes; }
            set
            {
                if (value == null || value.Length == 0)
                {
                    linestyledashes = DefaultLineStyleDashes;
                    LineStyle = Guppy2.GFX.LineStyle.Continuous;
                }
                else
                {
                    linestyledashes = value;
                    LineStyle = Guppy2.GFX.LineStyle.Custom;
                }
            }
        }

        private EndCap endcap=EndCap.Flat;
        public virtual EndCap EndCaps
        {
            get
            {
                return endcap;
            }
            set
            {
                endcap = value;
            }
        }

        private LineJoin linejoin=LineJoin.Bevel;
        public virtual LineJoin LineJoin
        {
            get
            {
                return linejoin;
            }
            set
            {
                linejoin = value;
            }
        }


        private Picture pattern;
        public virtual Picture Pattern
        {
            get { return pattern; }
            set { 
                pattern = value; 
                if (value != null) 
                    FillStyle = Guppy2.GFX.FillStyle.Pattern; 
                else 
                    FillStyle = Guppy2.GFX.FillStyle.Solid; 
            }
        }

        private Transform2d transform = Transform2d.Identity;
        public virtual Transform2d Transform
        {
            get { return transform; }
            set { transform = value ?? Transform2d.Identity; }
        }

        private Rectangle2i clip = null;
        public virtual Rectangle2i Clip
        {
            get { return clip; }
            set {
                if (value == null)
                {
                    clip = new Rectangle2i(0,0,Width-1,Height-1);
                }
                else
                {
                    //clip clipping rectangle to extents of the entire drawing surface
                    clip = new Rectangle2i(Math.Max(0, value.XMin), Math.Max(0, value.YMin), Math.Min(Width - 1, value.XMax), Math.Min(Height - 1, value.YMax));
                }
            } 
        }

        
        private Transform2d patterntransform = Transform2d.Identity;
        public virtual Transform2d PatternTransform
        {
            get
            {
                return patterntransform;
            }
            set
            {
                patterntransform = value ?? Transform2d.Identity;
            }
        }
        
        private FillMode fillmode=FillMode.EvenOdd;
        public virtual FillMode FillMode
        {
            get { return fillmode; }
            set { fillmode = value; }
        }
        
        double opacity=1.0;
        public virtual double Opacity
        {
            get { return opacity; }
            set { opacity = MathUtil.Clamp(value, 0.0, 1.0); }
        }

        public virtual void ResetAttributes()
        {
            Restore(GraphicAttributes.DefaultAttributes);
        }


        private string font = null;
        public virtual string Font
        {
            get { return font; }
            set { font = value; }
        }
        
        
        /// <summary>
        /// Gets info about the painter, for example which OpenGL driver is used for PainterGL etc.
        /// </summary>
        public abstract string Info { get; }


        private object tag = null;  //user data
        public virtual object Tag { get { return tag; } set { tag = value; } }
        

        #endregion ATTRIBUTES

        #region LINE_PRIMITIVES
        public abstract void SetPixel(int x, int y,int rgb);
        public abstract int GetPixel(int x, int y);

        public virtual void DrawRectangle(int x1, int y1, int x2, int y2) { DrawRectangle((double)x1, (double)y1, (double)x2, (double)y2); }
        public virtual void DrawRectangle(double x1, double y1, double x2, double y2) { DrawPolyLine(true, x1, y1, x2, y1, x2, y2, x1, y2); }
        public virtual void DrawRectangleT(double x1, double y1, double x2, double y2) {

            double xx1 = x1, yy1 = y1;
            double xx2 = x2, yy2 = y1;
            double xx3 = x2, yy3 = y2;
            double xx4 = x1, yy4 = y2;


            Transform.Apply(xx1, yy1, out xx1, out yy1, true);
            Transform.Apply(xx2, yy2, out xx2, out yy2, true);
            Transform.Apply(xx3, yy3, out xx3, out yy3, true);
            Transform.Apply(xx4, yy4, out xx4, out yy4, true);

            //check if rectangle becomes a line to avoid duplicate drawing
            if (GeomUtil.SquaredDistance(xx1, yy1, xx4, yy4) < 1.225)  //rect. is a horizontal line ( height<sqrt(1.5) )
                DrawLine(xx1, yy1, xx2, yy2);
            else if (GeomUtil.SquaredDistance(xx1, yy1, xx2, yy2) <= 1.225) //rect. is a verical line ( width<sqrt(1.5) )
                DrawLine(xx1, yy1, xx4, yy4);
            else
                DrawPolyLine(true, xx1, yy1, xx2, yy2, xx3, yy3, xx4, yy4);
        
        }

        public virtual void DrawLine(int x1, int y1, int x2, int y2) { DrawLine((double)x1, (double)y1, (double)x2, (double)y2); }
        public abstract void DrawLine(double x1, double y1, double x2, double y2);
        public virtual void DrawLineT(double x1, double y1, double x2, double y2)
        {
            Transform.Apply(x1, y1, out x1, out y1, true);
            Transform.Apply(x2, y2, out x2, out y2, true);
            DrawLine(x1, y1, x2, y2);
        }

        public virtual void DrawCircle(int cx, int cy, int radius) { DrawCircle((double)cx, (double)cy, (double)radius); }
        public virtual void DrawCircle(double cx, double cy, double radius) { DrawEllipse(cx, cy, radius, radius, 0.0); }
        public virtual void DrawCircleT(double cx, double cy, double radius) {
            var t = Transform;

            if (t.IsUniform)
            {
                t.Apply(cx, cy, out cx, out cy, true);
                radius *= Math.Sqrt(t.AX * t.AX + t.AY * t.AY);
                DrawCircle(cx, cy, radius);
                return;
            }
            
            //non unifor circle transform falls back on ellipse
            DrawEllipseT(cx, cy, radius, radius, 0.0); 
        }

        public virtual void DrawArc(int x1, int y1, int x2, int y2, double bulge)
        {
            DrawArc((double)x1, (double)y1, (double)x2, (double)y2,bulge);
        }

        public virtual void DrawArc(double x1, double y1, double x2, double y2, double bulge)
        {
            double cx, cy, rad;
            if (!GeomUtil.GetArcCenterFromBulge(x1, y1, x2, y2, bulge, out cx, out cy))
            {
                DrawLine(x1, y1, x2, y2);
                return;
            }

            rad = GeomUtil.GetArcRadiusFromBulge(x1, y1, x2, y2, bulge);
            DrawEllipticArc(cx, cy, rad, rad, 0.0, GeomUtil.Angle(cx, cy, x1, y1), GeomUtil.GetArcSweepAngleFromBulge(bulge));
        }

        public virtual void DrawArcT(double x1, double y1, double x2, double y2, double bulge)
        {
            var t=Transform;
            if (t.IsUniform)
            {
                //the arc will still be an arc after transform
                //transform it an draw with standrad arc function
                t.Apply(x1, y1, out x1, out y1, true);
                t.Apply(x2, y2, out x2, out y2, true);
                DrawArc(x1, y1, x2, y2, t.Determinant < 0.0 ? -bulge : bulge); //-buylge to support mirrored arcs
                return;
            }
            else
            { //the arc might change into an elliptic arc
                double cx, cy, rad;
                if (!GeomUtil.GetArcCenterFromBulge(x1, y1, x2, y2, bulge, out cx, out cy))
                {
                    //the arc is a line
                    DrawLineT(x1, y1, x2, y2);
                    return;
                }
                rad = GeomUtil.GetArcRadiusFromBulge(x1, y1, x2, y2, bulge);
                DrawEllipticArcT(cx, cy, rad, rad, 0.0, GeomUtil.Angle(cx, cy, x1, y1), GeomUtil.GetArcSweepAngleFromBulge(bulge));
            }
        }

        public virtual void DrawEllipse(int cx, int cy, int aradius, int bradius, double tilt) { DrawEllipse((double)cx, (double)cy, (double)aradius, (double)bradius, tilt); }
        public virtual void DrawEllipse(double cx, double cy, double aradius, double bradius, double tilt) { DrawEllipticArc(cx, cy, aradius, bradius, tilt, 0.0,MathUtil.Deg360); }
        public virtual void DrawEllipseT(double cx, double cy, double aradius, double bradius, double tilt) {
            bool circular = GeomUtil.TransformEllipse(ref cx, ref cy, ref aradius, ref bradius, ref tilt, Transform);

            if (circular) //draw with circle if possible=>faster
            {
                DrawCircle(cx, cy, aradius);
                return;
            }
            
            
            DrawEllipse(cx, cy, aradius, bradius, tilt);
        }

        public virtual void DrawEllipticArc(int cx, int cy, int aradius, int bradius, double tilt,double startangle,double sweepangle) { DrawEllipticArc((double)cx, (double)cy, (double)aradius, (double)bradius, tilt,startangle,sweepangle); }
        public abstract void DrawEllipticArc(double cx, double cy, double aradius, double bradius, double tilt, double startangle, double sweepangle);
        public virtual void DrawEllipticArcT(double cx, double cy, double aradius, double bradius, double tilt, double startangle, double sweepangle)
        {
            bool circular=GeomUtil.TransformEllipseArc(ref cx, ref cy, ref aradius, ref bradius, ref startangle,ref sweepangle,ref tilt, Transform);


        

            if (circular)
            {
                double bulge = GeomUtil.GetArcBulgeFromSweepAngle(sweepangle);
                double x1 = cx + Math.Cos(startangle) * aradius;
                double y1 = cy + Math.Sin(startangle) * aradius;
                double x2 = cx + Math.Cos(startangle + sweepangle) * aradius;
                double y2 = cy + Math.Sin(startangle + sweepangle) * aradius;
                DrawArc(x1, y1, x2, y2, bulge);
                return;
            }
            
            DrawEllipticArc(cx,cy,aradius,bradius,tilt,startangle,sweepangle);
        }

        public abstract void DrawPolyLine(bool close, params double[] xy);
        public virtual void DrawPolyLineT(bool close, params double[] xy)
        {
            var t = Transform;
            if (t != null && !t.IsIdentity) //transform xy array
            {
                int n = xy.Length;
                double x, y;
                double[] trxy = new double[n];


                n--; //protect from odd sized arrays
                for (int l = 0; l < n; l += 2)
                {
                    x = xy[l];
                    y = xy[l + 1];
                    t.Apply(x, y, out x, out y, true);
                    trxy[l] = x;
                    trxy[l + 1] = y;
                }

                xy = trxy;
            }

            DrawPolyLine(close, xy);
        }

        public abstract void DrawPath(GFXPath path);
        public abstract void DrawPathT(GFXPath path);

        #endregion LINE_PRIMITIVES

        #region FILLED_PRIMITIVES
        
        public virtual void FillRectangle(int x1, int y1, int x2, int y2) { FillRectangle((double)x1, (double)y1, (double)x2, (double)y2); }
        public virtual void FillRectangle(double x1, double y1, double x2, double y2) {
            GFXPath rc = new GFXPath();
            rc.MoveTo(x1, y1);
            rc.LineTo(x2, y1);
            rc.LineTo(x2, y2);
            rc.LineTo(x1, y2);
            rc.CloseSubPath();
            FillPath(rc);
        }

        public virtual void FillRectangleT(double x1, double y1, double x2, double y2)
        {
            GFXPath rc = new GFXPath();
            rc.MoveTo(x1, y1);
            rc.LineTo(x2, y1);
            rc.LineTo(x2, y2);
            rc.LineTo(x1, y2);
            rc.CloseSubPath();
            FillPathT(rc);
        }
        
        public virtual void FillGradientRectangle(int x1, int y1, int x2, int y2, int y1color, int y2color) { FillGradientRectangle((double)x1, (double)y1, (double)x2, (double)y2, y1color, y2color); }
        public abstract void FillGradientRectangle(double x1, double y1, double x2, double y2, int y1color, int y2color);

        public abstract void FillPath(GFXPath path);
        public abstract void FillPathT(GFXPath path);


        #endregion
        
        
        #region TEXT


        private static GFNFont FindFont(string fontname)
        {
            GFNFont res=null;
            string filename;

            if (string.IsNullOrEmpty(fontname))
                filename = "ISOCP";
            else if (fontname[0] == '*') //undecorate name that forces use of internal fonts
                filename = fontname.Substring(1).ToUpper();
            else
                filename = fontname.ToUpper();    //seems good as is


            //already loaded?
            if(loadedfonts.TryGetValue(filename,out res)) 
                return res;

            //not loaded, try to load it now
            Stream fontstream = StreamUtils.FindResourceStream(filename + ".gfn");
            if (fontstream == null)
            { //the font was not found, recursivly use isocp
                if (filename == "ISOCP") //avoid infinite recursion
                    throw new Exception("Fatal error: builtin font 'ISOCP' not found");
                res=FindFont("ISOCP");

                //remember the non-existing name maps to isocp font for future reference
                loadedfonts[filename] = res;
                
                return res;
            }

            res = GFNFont.FromStream(fontstream);
            fontstream.Close();
            loadedfonts[filename] = res;    //cache font for later use
            return res;
        }


        /// <summary>
        /// Computes how the font should be moved from baseline left to obey a specified alignment
        /// </summary>
        private void GetTextAlignDXDY(GFNFont fnt, string txt,double size, TextAlign align, out double dx, out double dy)
        {
            dx=0.0;
            dy=0.0;

            double w,h;

            //y align:
            if ((align & TextAlignAtom.TEXT_BOTTOM) != 0)
                dy = fnt.GetDescent(size);
            else if ((align & TextAlignAtom.TEXT_CENTER_Y) != 0)
                dy = -size * 0.5;
            else if ((align & TextAlignAtom.TEXT_TOP) != 0)
                dy = -fnt.GetAscent(size);
            //else align is baseline, dy=0.0

            //x align:
            if((align&TextAlignAtom.TEXT_RIGHT)!=0) {
                InternalGetTextSize(fnt,txt,size,out w,out h);
                dx=-w;
            }
            else if ((align & TextAlignAtom.TEXT_CENTER_X) != 0)
            {
                InternalGetTextSize(fnt, txt, size, out w, out h);
                dx = -w * 0.5;
            }
            //else align is left, dx=0.0 and we're done
        }

        private Transform2d GetTextPositionTransform(GFNFont fnt, string txt, double x, double y, double size, double angle, TextAlign align, Transform2d t)
        {
            //computes the transformation needed to transform text drawn at origo to get to the wanted position
            Transform2d res = Transform2d.Translate(x, y);
            if (angle != 0.0) res = Transform2d.Rotate(angle) * res;
            
            if (align != TextAlign.BaseLeft)
            {
                double dx, dy;
                GetTextAlignDXDY(fnt, txt, size, align, out dx, out dy);
                res = Transform2d.Translate(dx, dy) * res;
            }

            res = Transform2d.Scale(size/fnt.CapHeight) * res;

            if (t != null)
                return res * t;
            return res;
        }

        public virtual void DrawText(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            //TODO: make work with multiline text

            GFNFont fnt = FindFont(Font);   

            Transform2d tr = GetTextPositionTransform(fnt, txt, x, y, size, angle, align, null);
            var oldt = Transform;
            Transform = tr;
            fnt.DrawString(this, txt,tr);
            Transform = oldt;
        }

        public virtual void DrawTextT(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            //TODO: make work with multiline text

            GFNFont fnt = FindFont(Font);

            Transform2d tr = GetTextPositionTransform(fnt, txt, x, y, size, angle, align, null);
            var oldt = Transform;
            Transform = tr*oldt;
            fnt.DrawString(this, txt, tr);
            Transform = oldt;
        }


        private void InternalGetTextSize(GFNFont fnt,string txt, double size, out double w, out double h) {
             
            double lineheight = fnt.GetCellHeight(size);

            //compute size of multiline text
            double linewidth=0.0;
            w=0.0;
            h = lineheight;

            foreach (char ch in txt)
            {
                if (ch == '\n')
                {
                    linewidth = 0.0; //new line
                    w = Math.Max(w, linewidth);
                    h += lineheight;
                }
                else
                    linewidth += fnt.GetGlyphWidth(size, ch);
            }

            w = Math.Max(w, linewidth);
        }

        public virtual void GetTextSize(string txt, double size, out double w, out double h)
        {
            GFNFont fnt = FindFont(Font);
            InternalGetTextSize(fnt,txt,size,out w,out h);
        }

        public virtual GFXPath GetTextPath(string txt, double x, double y, double size, double angle, TextAlign align, Transform2d t)
        {
            GFNFont fnt = FindFont(Font);
            if(fnt==null)
                return null;

            Transform2d tr=GetTextPositionTransform(fnt, txt, x, y, size, angle, align, t);

            return fnt.GetTextPath(txt, tr);
        }

        public virtual double[] GetTextBounds(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            double w, h;

            GFNFont fnt = FindFont(Font);

            GetTextSize(txt, fnt.CapHeight, out w, out h);
            var t = GetTextPositionTransform(fnt, txt, x, y, size, angle, align, Transform2d.Identity);
            /*
            double d = fnt.GetDescent(fnt.CapHeight);
            double a = fnt.GetAscent(fnt.CapHeight);
            */
            double top = fnt.GetAscent(fnt.CapHeight);
            double bot = top - h;

            double[] res = new double[8] { 0, bot, w, bot, w, top, 0, top };
            t.Apply(res[0], res[1], out res[0], out res[1], true);
            t.Apply(res[2], res[3], out res[2], out res[3], true);
            t.Apply(res[4], res[5], out res[4], out res[5], true);
            t.Apply(res[6], res[7], out res[6], out res[7], true);

            return res;
        }
        
        public virtual double[] GetTextBox(string txt,double x,double y,double size,double angle,TextAlign align)
        {
            double[] pts=GetTextBounds(txt, x, y,size, angle,align);
            double xmin, xmax, ymin, ymax;
            xmin = Math.Min(pts[0],Math.Min(pts[2],Math.Min(pts[4],pts[6])));
            ymin = Math.Min(pts[1], Math.Min(pts[3], Math.Min(pts[5], pts[7])));
            xmax = Math.Max(pts[0], Math.Max(pts[2], Math.Max(pts[4], pts[6])));
            ymax = Math.Max(pts[1], Math.Max(pts[3], Math.Max(pts[5], pts[7])));
            return new double[] { xmin, ymin, xmax, ymax };
        }

        public virtual void GetFontDim(double size, out double linespace_pixels, out double ascent_pixels, out double descent_pixels,out bool filled)
        {
            GFNFont fnt = FindFont(Font);
            linespace_pixels = fnt.GetLineSpacing(size);
            ascent_pixels = fnt.GetAscent(size);
            descent_pixels = fnt.GetDescent(size);
            filled = fnt.Filled;
        }



        #endregion

        #region IMAGES

        protected void DrawIncompatiblePictureT(Picture p, double x, double y)
        {
            Picture compat = MakeCompatiblePicture(p);

            if (compat == null || compat==p)  //pictur could not be created or is same as already is incompatible
                return; //for safety

            DrawPictureT(compat, x, y);


            if (compat != p) //we actually created a new picture, free it
                compat.Dispose();
        }

        protected void DrawIncompatiblePicture(Picture p,double x,double y)
        {
            var oldt = Transform;
            Transform = Transform2d.Identity;
            DrawIncompatiblePictureT(p, x, y);
            Transform = oldt;
        }

        public abstract void DrawPicture(Picture p, double x, double y);
        public abstract void DrawPictureT(Picture p, double x, double y);


        /*public virtual void Blit(Picture p, int dest_x1, int dest_y1, int dest_x2, int dest_y2, int src_x, int src_y)
        {
            Picture newp = MakeCompatiblePicture(p);
            if (newp == p) //already compatible 
                return;


            Blit(p, dest_x1, dest_y1, dest_x2, dest_y2, src_x, src_y);
            newp.Dispose();
        }*/


        protected void BlitIncompatiblePicture(Picture p, int dst_x, int dst_y, int dst_w, int dst_h, int src_x, int src_y, int src_w, int src_h)
        {
            //blits an image that is incompatible. This is slow but used as fallback.
            Picture compat = MakeCompatiblePicture(p);
            if (compat == null || compat == p)
                return; //for safety, either no compatible picture could be created, or the returned image was the same as the sent one
            Blit(p, dst_x, dst_y, dst_w, dst_h, src_x, src_y, src_w, src_h);
        }

        public abstract void Blit(Picture p, int dst_x, int dst_y, int dst_w, int dst_h, int src_x, int src_y, int src_w, int src_h);

        
        
        public virtual Picture CreatePictureFromFile(string filename)
        {
            FileStream fs = null;
            Picture res = null;

            try
            {
                fs = File.OpenRead(filename);
                res = CreatePictureFromStream(fs);
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }

            return res;
        }

        public virtual Picture CreatePictureFromResource(string resname) { return CreatePictureFromResource(resname, Assembly.GetCallingAssembly());}
        
        public virtual Picture CreatePictureFromResource(string resname,Assembly asm)
        {
            Stream stream = StreamUtils.FindResourceStream(resname, asm);
            if (stream == null) return null;

            
            
            /*//try to get resource with the actual name
            Stream stream = asm.GetManifestResourceStream(resname);
            if (stream == null)
            {
                //get resource with some random namspace ending with the given name
                //this is good for Visual Studio express users, where resources are auto named
                string upname = "." + resname.ToUpper();
                foreach (string s in asm.GetManifestResourceNames())
                {
                    if (s.ToUpper().EndsWith(upname))
                    {
                        stream = asm.GetManifestResourceStream(s);
                        break;
                    }
                }
            }

            if (stream == null)
                return null;    //resource not found*/

            Picture res=null;
            try
            {
                res = CreatePictureFromStream(stream);
            }
            finally
            {
                stream.Close();
            }

            return res;

        }

        public abstract Picture CreatePictureFromSize(int width, int height);
        
        public abstract Picture CreatePictureFromStream(Stream str);

        public abstract Picture CreatePictureFromSoftPicture(SoftPicture img);

        public abstract Picture MakeCompatiblePicture(Picture p);

        /// <summary>
        /// Copies contents of the painter target into a picture. If pic argument is null, it is
        /// created. If area to copy is larger than picture,pic:s dimensions are to small it's resized/recreated to fit the entire
        /// </summary>
        /// <param name="pic"></param>
        public abstract void CopyPicture(ref Picture pic,int x1,int y1,int x2,int y2);


        
        #endregion


        #region MARKS

        public virtual void DrawMark(int x, int y, MarkType type, int size)
        {
            DrawMark((double)x, (double)y, type, size);
        }

        public virtual void DrawMarkT(double x, double y, MarkType type, int size)
        {
            Transform.Apply(x, y, out x, out y, true);
            DrawMark(x, y, type, size);
        }
        
        public virtual void DrawMark(double x, double y, MarkType type, int size)
        {
            double sz = (double)size;

            FillStyle oldfill = FillStyle;

            if ((type & MarkType.LargeCross) != 0)
            {
                double sd = sz * 1.414213562; //ie. sqrt(2)
                DrawLine(x, y - sd, x, y + sd);
                DrawLine(x - sd, y, x + sd, y);
            }
            else if ((type & MarkType.Cross) != 0)
            {
                DrawLine(x, y - sz, x, y + sz);
                DrawLine(x - sz, y, x + sz, y);
            }
            else if ((type & MarkType.Slit) != 0)
                DrawLine(x, y, x, y + sz);
            else if ((type & MarkType.Pixel) != 0)
                SetPixel(GFXUtil.FloatToInt(x),GFXUtil.FloatToInt(y),Color);


            if ((type & MarkType.DiagonalCross) != 0)
            {
                DrawLine(x - sz, y - sz, x + sz, y + sz);
                DrawLine(x - sz, y + sz, x + sz, y - sz);
            }


            if ((type & MarkType.Triangle) != 0)
            {
                DrawLine(x - sz, y - sz, x + sz, y - sz);
                DrawLine(x + sz, y - sz, x , y + sz);
                DrawLine(x , y + sz, x-sz, y - sz);
            }

            if ((type & MarkType.Roof) != 0)
                DrawLine(x - sz, y + sz, x + sz, y + sz);

            if ((type & MarkType.Floor) != 0)
                DrawLine(x - sz, y - sz, x + sz, y - sz);            

            if ((type & MarkType.Circle) != 0)
                DrawCircle(x, y, sz);

            
            if((type&MarkType.Diamond)!=0) 
                DrawPolyLine(true,x+sz,y,  x,y-sz,  x-sz,y,  x,y+sz);
            

            if ((type & MarkType.FilledRectangle) != 0)
            {
                FillStyle = FillStyle.Solid;
                FillRectangle(x - sz, y - sz, x + sz, y + sz);
            }
            else if ((type & MarkType.Rectangle) != 0)
                DrawRectangle(x - sz, y - sz, x + sz, y + sz);

            
            if (type.HasFlag(MarkType.Perpendicular))
            {
                DrawLine(x - sz, y + sz, x - sz, y - sz);
                DrawLine(x - sz, y - sz, x + sz, y - sz);
                DrawLine(x - sz, y, x, y);
                DrawLine(x, y, x, y - sz);
            }

            if (type.HasFlag(MarkType.Ellipsis))
            {
                MarkEllipsisDot(x - sz, y - sz);
                MarkEllipsisDot(x , y - sz);
                MarkEllipsisDot(x + sz, y - sz);

            }
           

            FillStyle = oldfill;
        }

        private void MarkEllipsisDot(double rx, double ry)
        {
            int x = GFXUtil.FloatToInt(rx);
            int y = GFXUtil.FloatToInt(ry);

            SetPixel(x, y, Color);
            SetPixel(x+1, y, Color);
            SetPixel(x + 1, y+1, Color);
            SetPixel(x, y + 1, Color);
        }

        #endregion

        #region MISC  ////////////////////////////////////////////////////////////////////////////////////////////////

        public virtual void Flush() { }

        public abstract void Clear(int rgb);
        
        public abstract int Width { get; }
        
        public abstract int Height { get; }
        
        public abstract void MillimetersToPixels(double mmx, double mmy, out double pixx, out double pixy);
        
        public virtual void PixelsToMillimeters(double pixx, double pixy, out double mmx, out double mmy)
        {
            double mppx, mppy;
            MillimetersToPixels(1.0, 1.0, out mppx, out mppy);
            mmx = pixx / mppx;
            mmy = pixy / mppy;
        }

        public virtual GraphicAttributes Save()
        {
            GraphicAttributes ga = new GraphicAttributes(this);
            return ga;
        }

        public virtual void Restore(GraphicAttributes ga)
        {
            if (ga != null)
                ga.Restore(this);
        }
        
        #endregion  ////////////////////////////////////////////////////////////////////////////////////////////////

        
        public abstract void Dispose();
        
    }


    public class GraphicAttributes
    {
        public readonly int Color;
        public readonly double Opacity;
        public readonly bool AntiGrain;
        public readonly Hatch Hatch;
        public readonly FillStyle FillStyle;
        public readonly double LineWidth;
        public readonly LineStyle LineStyle;
        public readonly double[] LineStyleDashes;
        public readonly EndCap EndCaps;
        public readonly LineJoin LineJoin;
        public readonly Picture Pattern;
        public readonly Transform2d Transform;
        public readonly Rectangle2i Clip;
        public readonly Transform2d PatternTransform;
        public readonly FillMode FillMode;
        public readonly string Font = null;


        public static readonly GraphicAttributes DefaultAttributes = new GraphicAttributes();

        /// <summary>
        /// Default constructor sets all attributes to their default value.
        /// Use DefaultAttributes to access an instance of this.
        /// </summary>
        private GraphicAttributes()
        {
            Color = RGB.Black;
            Opacity = 1.0;
            AntiGrain = false;
            Hatch = Hatch.Cross;
            FillStyle = FillStyle.Solid;
            LineWidth = 0.0;
            LineStyle = LineStyle.Continuous;
            LineStyleDashes = Painter.DefaultLineStyleDashes;
            EndCaps = EndCap.Flat;
            LineJoin = LineJoin.Bevel;
            Pattern = null;
            Transform = Transform2d.Identity;
            Clip = null; 
            PatternTransform = Transform2d.Identity;
            FillMode = FillMode.EvenOdd;
            Font = null;
        }

        public GraphicAttributes(Painter p)
        {
            Color = p.Color;
            Opacity = p.Opacity;
            AntiGrain = p.AntiGrain;
            Hatch = p.Hatch;
            FillStyle = p.FillStyle;
            LineWidth = p.LineWidth;
            LineStyle = p.LineStyle;
            LineStyleDashes = p.LineStyleDashes;
            EndCaps = p.EndCaps;
            LineJoin = p.LineJoin;
            Pattern = p.Pattern;
            Transform = p.Transform;
            Clip = p.Clip;
            PatternTransform = p.PatternTransform;
            FillMode = p.FillMode;
            Font = p.Font;
        }

        public virtual void Restore(Painter p) {
            
            p.Color=Color;
            p.Opacity = Opacity;
            p.AntiGrain=AntiGrain;

            p.Pattern = Pattern;
            p.Hatch=Hatch;
            p.FillStyle=FillStyle; //must set fillstyle after setting atributes that changes it automatically
            
            p.LineStyleDashes = LineStyleDashes;    //must set before linestyle since linestyle is changed auto. by this setter
            p.LineStyle=LineStyle;

            p.LineWidth = LineWidth;
            p.EndCaps=EndCaps;
            p.LineJoin=LineJoin;
            p.Transform = Transform;
            p.Clip = Clip;
            p.PatternTransform = PatternTransform;
            p.FillMode = FillMode;
            p.Font = Font;
        }
    }

    
    public abstract class Picture:IDisposable
    {
        public abstract int Width { get;  }
        public abstract int Height { get; }
        public abstract SoftPicture ToSoftPicture();
        public abstract void Dispose();
        public abstract object NativeObject { get; }
        public abstract Painter CreatePainter();
    }
    
}


    