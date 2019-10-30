using Guppy2.Calc.Geom2d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GFX
{

    [Flags]
    public enum MetafilePlayFlags
    {
        None=0,
        NoColorChange = 1,
        NoOpacityChange=2,
        NoLineStyleChange=4,
        NoFillStyleChange=8

    }

    internal interface IMetafileRecord
    {
        void Play(Painter target,MetafilePlayFlags flags);
    }

    public class PictureMetafile
    {
        List<IMetafileRecord> records = new List<IMetafileRecord>();


        internal void Add(IMetafileRecord rec) { records.Add(rec); }


        internal void Clear() { records.Clear(); }

        public void Play(Painter target,MetafilePlayFlags flags)
        {
            if (target != null)
            {
                foreach (var rec in records)
                    rec.Play(target,flags);
            }
        }
    }

    public class PainterMetafile : Painter
    {
        PictureMetafile records;

        public PainterMetafile(PictureMetafile target)
        {
            records = target;
        }


        #region ATTRIBUTES

        public override string Info
        {
            get { return "Metafile"; }
        }

        public override int Color
        {
            get
            {
                return base.Color;
            }
            set
            {
                if (value != Color)
                    records.Add(new MetafileRecordColor(value));

                base.Color = value;
            }
        }

        public override double Opacity
        {
            get
            {
                return base.Opacity;
            }
            set
            {
                if (Opacity != value)
                    records.Add(new MetafileRecordOpacity(value));
                base.Opacity = value;
            }
        }

        public override double LineWidth
        {
            get
            {
                return base.LineWidth;
            }
            set
            {
                if (LineWidth != value)
                    records.Add(new MetafileRecordLineWidth(value));
                base.LineWidth = value;
            }
        }

        public override bool AntiGrain
        {
            get
            {
                return base.AntiGrain;
            }
            set
            {
                if (AntiGrain != value)
                    records.Add(new MetafileRecordAntiGrain(value));
                base.AntiGrain = value;
            }
        }

        public override Hatch Hatch
        {
            get
            {
                return base.Hatch;
            }
            set
            {
                if (value != Hatch)
                    records.Add(new MetafileRecordHatch(value));
                base.Hatch = value;
            }
        }

        public override FillStyle FillStyle
        {
            get
            {
                return base.FillStyle;
            }
            set
            {
                if (value != FillStyle)
                    records.Add(new MetafileRecordFillStyle(value));
                base.FillStyle = value;
            }
        }

        public override LineStyle LineStyle
        {
            get
            {
                return base.LineStyle;
            }
            set
            {
                if (value != LineStyle)
                    records.Add(new MetafileRecordLineStyle(value));
                base.LineStyle = value;
            }
        }


        public override double[] LineStyleDashes
        {
            get
            {
                return base.LineStyleDashes;
            }
            set
            {
                if (value != LineStyleDashes)
                    records.Add(new MetafileRecordLineStyleDashes(value));
                base.LineStyleDashes = value;
            }
        }

        public override EndCap EndCaps
        {
            get
            {
                return base.EndCaps;
            }
            set
            {
                if (value != EndCaps)
                    records.Add(new MetafileRecordEndCaps(value));
                base.EndCaps = value;
            }
        }

        public override LineJoin LineJoin
        {
            get
            {
                return base.LineJoin;
            }
            set
            {
                if (value != LineJoin)
                    records.Add(new MetafileRecordLineJoin(value));
                base.LineJoin = value;
            }
        }

        public override Picture Pattern
        {
            get
            {
                return base.Pattern;
            }
            set
            {
                if (value != Pattern)
                    records.Add(new MetafileRecordPattern(value));
                base.Pattern = value;
            }
        }

        public override Transform2d Transform
        {
            get
            {
                return base.Transform;
            }
            set
            {
                if (value != Transform)
                    records.Add(new MetafileRecordTransform(value));
                base.Transform = value;
            }
        }

        public override Transform2d PatternTransform
        {
            get
            {
                return base.PatternTransform;
            }
            set
            {
                if (value != PatternTransform)
                    records.Add(new MetafileRecordPatternTransform(value));
                base.PatternTransform = value;
            }
        }

        public override FillMode FillMode
        {
            get
            {
                return base.FillMode;
            }
            set
            {
                if (value != FillMode)
                    records.Add(new MetafileRecordFillMode(value));
                base.FillMode = value;
            }
        }


        #endregion

        #region LINE_PRIMITIVES

        public override void SetPixel(int x, int y, int rgb)
        {
            records.Add(new MetafileRecordSetPixel(x, y, rgb));
        }

        public override int GetPixel(int x, int y)
        {
            return 0;
        }

        public override void DrawRectangle(double x1, double y1, double x2, double y2)
        {
            records.Add(new MetafileRecordRectangle(x1, y1, x2, y2));
        }

        public override void DrawRectangleT(double x1, double y1, double x2, double y2)
        {
            records.Add(new MetafileRecordRectangleT(x1, y1, x2, y2));
        }

        public override void DrawLine(double x1, double y1, double x2, double y2)
        {
            records.Add(new MetafileRecordLine(x1, y1, x2, y2));
        }

        public override void DrawLineT(double x1, double y1, double x2, double y2)
        {
            records.Add(new MetafileRecordLineT(x1, y1, x2, y2));
        }

        public override void DrawCircle(double cx, double cy, double radius)
        {
            records.Add(new MetafileRecordCircle(cx, cy, radius));
        }

        public override void DrawCircleT(double cx, double cy, double radius)
        {
            records.Add(new MetafileRecordCircleT(cx, cy, radius));
        }

        public override void DrawArc(double x1, double y1, double x2, double y2, double bulge)
        {
            records.Add(new MetafileRecordArc(x1, y1, x2, y2, bulge));
        }

        public override void DrawArcT(double x1, double y1, double x2, double y2, double bulge)
        {
            records.Add(new MetafileRecordArcT(x1, y1, x2, y2, bulge));
        }

        public override void DrawEllipse(double cx, double cy, double aradius, double bradius, double tilt)
        {
            records.Add(new MetafileRecordEllipse(cx, cy, aradius, bradius, tilt));
        }

        public override void DrawEllipseT(double cx, double cy, double aradius, double bradius, double tilt)
        {
            records.Add(new MetafileRecordEllipseT(cx, cy, aradius, bradius, tilt));
        }

        public override void DrawEllipticArc(double cx, double cy, double aradius, double bradius, double tilt, double startangle, double sweepangle)
        {
            records.Add(new MetafileRecordEllipticArc(cx, cy, aradius, bradius, tilt, startangle, sweepangle));
        }

        public override void DrawEllipticArcT(double cx, double cy, double aradius, double bradius, double tilt, double startangle, double sweepangle)
        {
            records.Add(new MetafileRecordEllipticArcT(cx, cy, aradius, bradius, tilt, startangle, sweepangle));
        }

        public override void DrawPolyLine(bool close, params double[] xy)
        {
            records.Add(new MetafileRecordPolyLine(close, xy));
        }

        public override void DrawPolyLineT(bool close, params double[] xy)
        {
            records.Add(new MetafileRecordPolyLineT(close, xy));
        }

        public override void DrawPath(GFXPath path)
        {
            records.Add(new MetafileRecordPath(path));
        }

        public override void DrawPathT(GFXPath path)
        {
            records.Add(new MetafileRecordPathT(path));
        }

        #endregion LINE_PRIMITIVES

        #region FILLED_PRIMITIVES


        public override void FillRectangle(double x1, double y1, double x2, double y2)
        {
            records.Add(new MetafileRecordFillRectangle(x1, y1, x2, y2));
        }

        public override void FillRectangleT(double x1, double y1, double x2, double y2)
        {
            records.Add(new MetafileRecordFillRectangleT(x1, y1, x2, y2));
        }

        public override void FillGradientRectangle(double x1, double y1, double x2, double y2, int y1color, int y2color)
        {
            records.Add(new MetafileRecordFillGradientRectangle(x1, y1, x2, y2, y1color, y2color));
        }

        public override void FillPath(GFXPath path)
        {
            records.Add(new MetafileRecordFillPath(path));
        }

        public override void FillPathT(GFXPath path)
        {
            records.Add(new MetafileRecordFillPathT(path));
        }

        #endregion


        #region TEXT

       
        public override void DrawText(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            records.Add(new MetafileRecordText(txt, x, y, size, angle, align));
        }

        public override void DrawTextT(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            records.Add(new MetafileRecordTextT(txt, x, y, size, angle, align));
        }

        public override GFXPath GetTextPath(string txt, double x, double y, double size, double angle, TextAlign align, Transform2d t)
        {
            return null;    //not possible in this painter
        }
        
        public override void GetTextSize(string txt, double size, out double w, out double h)
        {
            //not available in this painter but we take a wild guess
            w = txt.Length * size;
            h = size;
        }

        public override double[] GetTextBounds(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            return null;    //not in this painter
        }

        public override void GetFontDim(double size, out double linespace_pixels, out double ascent_pixels, out double descent_pixels,out bool filled)
        {
            linespace_pixels = size * 1.5; //not really supported, use somewhat reasonable values
            ascent_pixels = size * 0.8;
            descent_pixels = size * 0.2;
            filled = false;
        }
        
        #endregion TEXT

        #region IMAGES

        public override Picture CreatePictureFromSize(int width, int height)
        {
            return null; //not available in this painter
        }

        public override Picture CreatePictureFromStream(System.IO.Stream str)
        {
            return null; //not available in this painter
        }

        public override Picture CreatePictureFromSoftPicture(SoftPicture img)
        {
            return img;
        }

        public override Picture MakeCompatiblePicture(Picture p)
        {
            return p;
        }

        public override void CopyPicture(ref Picture pic, int x1, int y1, int x2, int y2)
        {
            //not available in this painter
        }

        public override void DrawPicture(Picture p, double x, double y)
        {
            records.Add(new MetafileRecordPicture(p, x, y));
        }

        public override void DrawPictureT(Picture p, double x, double y)
        {
            records.Add(new MetafileRecordPictureT(p, x, y));
        }

        /*public override void Blit(Picture p, int dest_x1, int dest_y1, int dest_x2, int dest_y2, int src_x, int src_y)
        {
            records.Add(new MetafileRecordBlit(p, dest_x1, dest_y1, dest_x2, dest_y2, src_x, src_y));
        }*/

        public override void Blit(Picture p, int dst_x, int dst_y, int dst_w, int dst_h, int src_x, int src_y, int src_w, int src_h)
        {
            records.Add(new MetafileRecordBlit(p, dst_x, dst_y, dst_w, dst_h, src_x, src_y, src_w, src_h));
        }

        #endregion


        #region MARKS

        public override void DrawMark(double x, double y, MarkType type, int size)
        {

            records.Add(new MetafileRecordMark(x, y, type, size));
        }

        public override void DrawMarkT(double x, double y, MarkType type, int size)
        {
            records.Add(new MetafileRecordMarkT(x, y, type, size));
        }

        #endregion


        #region MISC

        public override void Clear(int rgb)
        {
            records.Clear();
        }

        public override int Width
        {
            get
            {
                return 0; //not available in this painter 
            }
        }

        public override int Height
        {
            get
            {
                return 0; //not available in this painter 
            }
        }

        public override void MillimetersToPixels(double mmx, double mmy, out double pixx, out double pixy)
        {
            pixx = mmx;
            pixy = mmy;
        }

        public override void Dispose()
        {

        }

        #endregion MISC



        #region METAFILE_RECORDS




        #region ATTRIBUTE_RECORDS

        internal class MetafileRecordColor : IMetafileRecord
        {
            int rgb;

            public MetafileRecordColor(int rgb)
            {
                this.rgb = rgb;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                if(!flags.HasFlag(MetafilePlayFlags.NoColorChange))
                    target.Color = rgb;
            }
        }

        internal class MetafileRecordOpacity : IMetafileRecord
        {
            double opacity;

            public MetafileRecordOpacity(double opacity)
            {
                this.opacity = opacity;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                if(!flags.HasFlag(MetafilePlayFlags.NoOpacityChange))
                    target.Opacity = opacity;
            }
        }

        internal class MetafileRecordAntiGrain : IMetafileRecord
        {
            bool antigrain;

            public MetafileRecordAntiGrain(bool a)
            {
                this.antigrain = a;
            }

            public void Play(Painter target,MetafilePlayFlags flags)
            {
                target.AntiGrain = antigrain;
            }
        }

        internal class MetafileRecordHatch : IMetafileRecord
        {
            Hatch hatch;

            public MetafileRecordHatch(Hatch h)
            {
                this.hatch = h;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                target.Hatch = hatch;
            }
        }

        internal class MetafileRecordFillStyle : IMetafileRecord
        {
            FillStyle style;

            public MetafileRecordFillStyle(FillStyle fs)
            {
                this.style = fs;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                if(!flags.HasFlag(MetafilePlayFlags.NoFillStyleChange))
                    target.FillStyle = style;
            }
        }

        internal class MetafileRecordLineWidth : IMetafileRecord
        {
            double lwt;

            public MetafileRecordLineWidth(double lwt)
            {
                this.lwt = lwt;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                target.LineWidth = lwt;
            }
        }


        internal class MetafileRecordLineStyle : IMetafileRecord
        {
            LineStyle style;

            public MetafileRecordLineStyle(LineStyle ls)
            {
                this.style = ls;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                if(!flags.HasFlag(MetafilePlayFlags.NoLineStyleChange))
                    target.LineStyle = style;
            }
        }

        internal class MetafileRecordLineStyleDashes : IMetafileRecord
        {
            double[] dashes;

            public MetafileRecordLineStyleDashes(double[] dashes)
            {
                this.dashes = dashes;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                target.LineStyleDashes = dashes;
            }

        }

        internal class MetafileRecordEndCaps : IMetafileRecord
        {
            EndCap cap;

            public MetafileRecordEndCaps(EndCap cap)
            {
                this.cap = cap;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                target.EndCaps = cap;
            }
        }

        internal class MetafileRecordLineJoin : IMetafileRecord
        {
            LineJoin join;

            public MetafileRecordLineJoin(LineJoin join)
            {
                this.join = join;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                target.LineJoin = join;
            }
        }

        internal class MetafileRecordPattern : IMetafileRecord
        {
            Picture pattern;

            public MetafileRecordPattern(Picture pattern)
            {
                this.pattern = pattern;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                target.Pattern = pattern;
            }
        }


        internal class MetafileRecordTransform : IMetafileRecord
        {
            Transform2d t;

            public MetafileRecordTransform(Transform2d trans)
            {
                this.t = trans;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                target.Transform = t;
            }
        }

        internal class MetafileRecordPatternTransform : IMetafileRecord
        {
            Transform2d t;

            public MetafileRecordPatternTransform(Transform2d trans)
            {
                this.t = trans;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                target.PatternTransform = t;
            }
        }


        internal class MetafileRecordFillMode : IMetafileRecord
        {
            FillMode fm;

            public MetafileRecordFillMode(FillMode fm)
            {
                this.fm = fm;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                target.FillMode = fm;
            }
        }

        #endregion

        #region LINE_PRIMITIVE_RECORDS


        internal class MetafileRecordSetPixel : IMetafileRecord
        {
            int x, y, rgb;
            public MetafileRecordSetPixel(int x, int y, int rgb)
            {
                this.x = x;
                this.y = y;
                this.rgb = rgb;
            }

            public virtual void Play(Painter target, MetafilePlayFlags flags)
            {
                target.SetPixel(x, y, rgb);
            }
        }

        internal class MetafileRecordLine : IMetafileRecord
        {
            protected double x1, y1, x2, y2;
            public MetafileRecordLine(double x1, double y1, double x2, double y2)
            {
                this.x1 = x1;
                this.y1 = y1;
                this.x2 = x2;
                this.y2 = y2;
            }

            public virtual void Play(Painter target, MetafilePlayFlags flags)
            {
                target.DrawLine(x1, y1, x2, y2);
            }
        }


        internal class MetafileRecordLineT : MetafileRecordLine
        {
            public MetafileRecordLineT(double x1, double y1, double x2, double y2) : base(x1, y1, x2, y2) { }
            public override void Play(Painter target, MetafilePlayFlags flags) { target.DrawLineT(x1, y1, x2, y2); }
        }

        internal class MetafileRecordRectangle : IMetafileRecord
        {
            protected double x1, y1, x2, y2;
            public MetafileRecordRectangle(double x1, double y1, double x2, double y2)
            {
                this.x1 = x1;
                this.y1 = y1;
                this.x2 = x2;
                this.y2 = y2;
            }

            public virtual void Play(Painter target, MetafilePlayFlags flags)
            {
                target.DrawRectangle(x1, y1, x2, y2);
            }
        }


        internal class MetafileRecordRectangleT : MetafileRecordRectangle
        {
            public MetafileRecordRectangleT(double x1, double y1, double x2, double y2) : base(x1, y1, x2, y2) { }
            public override void Play(Painter target, MetafilePlayFlags flags) { target.DrawRectangleT(x1, y1, x2, y2); }
        }

        internal class MetafileRecordCircle : IMetafileRecord
        {
            protected double x1, y1, radius;

            public MetafileRecordCircle(double x, double y, double radius)
            {
                this.x1 = x;
                this.y1 = y;
                this.radius = radius;
            }

            public virtual void Play(Painter target, MetafilePlayFlags flags)
            {
                target.DrawCircle(x1, y1, radius);
            }
        }


        internal class MetafileRecordCircleT : MetafileRecordCircle
        {
            public MetafileRecordCircleT(double x, double y, double rad) : base(x, y, rad) { }
            public override void Play(Painter target, MetafilePlayFlags flags) { target.DrawCircleT(x1, y1, radius); }
        }


        internal class MetafileRecordArc : IMetafileRecord
        {
            protected double x1, y1, x2, y2, bulge;
            public MetafileRecordArc(double x1, double y1, double x2, double y2, double bulge)
            {
                this.x1 = x1;
                this.y1 = y1;
                this.x2 = x2;
                this.y2 = y2;
                this.bulge = bulge;
            }

            public virtual void Play(Painter target, MetafilePlayFlags flags)
            {
                target.DrawArc(x1, y1, x2, y2, bulge);
            }
        }


        internal class MetafileRecordArcT : MetafileRecordArc
        {
            public MetafileRecordArcT(double x1, double y1, double x2, double y2, double bulge) : base(x1, y1, x2, y2, bulge) { }
            public override void Play(Painter target, MetafilePlayFlags flags) { target.DrawArcT(x1, y1, x2, y2, bulge); }
        }

        internal class MetafileRecordEllipse : IMetafileRecord
        {
            protected double x, y, aradius, bradius, tilt;

            public MetafileRecordEllipse(double x, double y, double arad, double brad, double tilt)
            {
                this.x = x;
                this.y = y;
                this.aradius = arad;
                this.bradius = brad;
                this.tilt = tilt;
            }

            public virtual void Play(Painter target, MetafilePlayFlags flags)
            {
                target.DrawEllipse(x, y, aradius, bradius, tilt);
            }
        }


        internal class MetafileRecordEllipseT : MetafileRecordEllipse
        {
            public MetafileRecordEllipseT(double x, double y, double arad, double brad, double tilt) : base(x, y, arad, brad, tilt) { }
            public override void Play(Painter target, MetafilePlayFlags flags) { target.DrawEllipseT(x, y, aradius, bradius, tilt); }
        }

        internal class MetafileRecordEllipticArc : IMetafileRecord
        {
            protected double x, y, aradius, bradius, tilt, startangle, sweepangle;

            public MetafileRecordEllipticArc(double x, double y, double arad, double brad, double tilt, double startangle, double sweepangle)
            {
                this.x = x;
                this.y = y;
                this.aradius = arad;
                this.bradius = brad;
                this.startangle = startangle;
                this.sweepangle = sweepangle;
                this.tilt = tilt;

            }

            public virtual void Play(Painter target, MetafilePlayFlags flags)
            {
                target.DrawEllipticArc(x, y, aradius, bradius, tilt, startangle, sweepangle);
            }
        }


        internal class MetafileRecordEllipticArcT : MetafileRecordEllipticArc
        {
            public MetafileRecordEllipticArcT(double x, double y, double arad, double brad, double tilt, double startangle, double sweepangle) : base(x, y, arad, brad, tilt, startangle, sweepangle) { }
            public override void Play(Painter target, MetafilePlayFlags flags) { target.DrawEllipticArcT(x, y, aradius, bradius, tilt, startangle, sweepangle); }
        }

        internal class MetafileRecordPolyLine : IMetafileRecord
        {
            protected double[] xy;
            protected bool close;

            public MetafileRecordPolyLine(bool close, double[] xy)
            {
                this.close = close;
                this.xy = xy;
            }

            public virtual void Play(Painter target, MetafilePlayFlags flags)
            {
                target.DrawPolyLine(close, xy);
            }
        }


        internal class MetafileRecordPolyLineT : MetafileRecordPolyLine
        {
            public MetafileRecordPolyLineT(bool close, double[] xy) : base(close, xy) { }
            public override void Play(Painter target, MetafilePlayFlags flags) { target.DrawPolyLineT(close, xy); }
        }

        internal class MetafileRecordPath : IMetafileRecord
        {
            protected GFXPath path;

            public MetafileRecordPath(GFXPath path)
            {
                this.path = path;
            }

            public virtual void Play(Painter target, MetafilePlayFlags flags)
            {
                target.DrawPath(path);
            }
        }


        internal class MetafileRecordPathT : MetafileRecordPath
        {
            public MetafileRecordPathT(GFXPath path) : base(path) { }
            public override void Play(Painter target, MetafilePlayFlags flags) { target.DrawPathT(path); }
        }

        #endregion LINE_PRIMITIVE_RECORDS


        #region FILLED_PRIMITIVE_RECORDS

        internal class MetafileRecordFillRectangle : IMetafileRecord
        {
            protected double x1, y1, x2, y2;
            public MetafileRecordFillRectangle(double x1, double y1, double x2, double y2)
            {
                this.x1 = x1;
                this.y1 = y1;
                this.x2 = x2;
                this.y2 = y2;
            }

            public virtual void Play(Painter target, MetafilePlayFlags flags)
            {
                target.FillRectangle(x1, y1, x2, y2);
            }
        }


        internal class MetafileRecordFillRectangleT : MetafileRecordFillRectangle
        {
            public MetafileRecordFillRectangleT(double x1, double y1, double x2, double y2) : base(x1, y1, x2, y2) { }
            public override void Play(Painter target, MetafilePlayFlags flags) { target.FillRectangleT(x1, y1, x2, y2); }
        }

        internal class MetafileRecordFillGradientRectangle : MetafileRecordFillRectangle
        {
            int y1color;
            int y2color;

            public MetafileRecordFillGradientRectangle(double x1, double y1, double x2, double y2, int y1color, int y2color)
                : base(x1, y1, x2, y2)
            {
                this.y1color = y1color;
                this.y2color = y2color;
            }

            public override void Play(Painter target, MetafilePlayFlags flags) { target.FillGradientRectangle(x1, y1, x2, y2, y1color, y2color); }
        }


        internal class MetafileRecordFillPath : IMetafileRecord
        {
            protected GFXPath path;

            public MetafileRecordFillPath(GFXPath path)
            {
                this.path = path;
            }

            public virtual void Play(Painter target, MetafilePlayFlags flags)
            {
                target.FillPath(path);
            }
        }


        internal class MetafileRecordFillPathT : MetafileRecordFillPath
        {
            public MetafileRecordFillPathT(GFXPath path) : base(path) { }
            public override void Play(Painter target, MetafilePlayFlags flags) { target.FillPathT(path); }
        }


        #endregion FILLED_PRIMITIVE_RECORDS

        #region TEXT_RECORDS

        internal class MetafileRecordText : IMetafileRecord
        {
            string txt;
            double x;
            double y;
            double size;
            double angle;
            TextAlign align;

            public MetafileRecordText(string txt, double x, double y, double size, double angle, TextAlign align)
            {
                this.txt = txt;
                this.x = x;
                this.y = y;
                this.size = size;
                this.angle = angle;
                this.align = align;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                target.DrawText(txt, x, y, size, angle, align);
            }
        }

        internal class MetafileRecordTextT : IMetafileRecord
        {
            string txt;
            double x;
            double y;
            double size;
            double angle;
            TextAlign align;

            public MetafileRecordTextT(string txt, double x, double y, double size, double angle, TextAlign align)
            {
                this.txt = txt;
                this.x = x;
                this.y = y;
                this.size = size;
                this.angle = angle;
                this.align = align;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                target.DrawTextT(txt, x, y, size, angle, align);
            }
        }

        #endregion

        #region IMAGE_RECORDS

        internal class MetafileRecordPicture : IMetafileRecord
        {
            Picture pic;
            double x, y;

            public MetafileRecordPicture(Picture pic, double x, double y)
            {
                this.pic = pic;
                this.x = x;
                this.y = y;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                target.DrawPicture(pic, x, y);
            }
        }

        internal class MetafileRecordPictureT : IMetafileRecord
        {
            Picture pic;
            double x, y;

            public MetafileRecordPictureT(Picture pic, double x, double y)
            {
                this.pic = pic;
                this.x = x;
                this.y = y;
            }

            public void Play(Painter target, MetafilePlayFlags flags)
            {
                target.DrawPictureT(pic, x, y);
            }
        }

        internal class MetafileRecordBlit : IMetafileRecord
        {
            int dst_x, dst_y, dst_w, dst_h, src_x, src_y,src_w,src_h;
            Picture pic;

            public MetafileRecordBlit(Picture pic, int dst_x,int dst_y,int dst_w,int dst_h,int src_x,int src_y,int src_w,int src_h)
            {
                this.pic = pic;
                this.dst_x = dst_x;
                this.dst_y = dst_y;
                this.dst_w = dst_w;
                this.dst_h = dst_h;
                this.src_x = src_x;
                this.src_y = src_y;
                this.src_w = src_w;
                this.src_h = src_h;
            }


            public void Play(Painter target, MetafilePlayFlags flags)
            {
                target.Blit(pic, dst_x, dst_y, dst_w, dst_h, src_x, src_y,src_w,src_h);
            }
        }

        #endregion IMAGE_RECORDS


        #region MARK_RECORDS

        internal class MetafileRecordMark : IMetafileRecord
        {
            protected double x,y;
            protected MarkType type;
            protected int size;

            public MetafileRecordMark(double x,double y,MarkType type,int size)
            {
                this.x = x;
                this.y = y;
                this.type = type;
                this.size = size;
            }

            public virtual void Play(Painter target, MetafilePlayFlags flags)
            {
                target.DrawMark(x, y, type, size);
            }
        }


        internal class MetafileRecordMarkT : MetafileRecordMark
        {
            public MetafileRecordMarkT(double x, double y, MarkType type, int size) : base(x,y,type,size) { }
            public override void Play(Painter target, MetafilePlayFlags flags) { target.DrawMarkT(x,y,type,size); }
        }

        #endregion MARK_RECORDS


        #endregion METAFILE_RECORDS
    }
}
