using Guppy2.Calc.Geom2d;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Guppy2.GFX
{
    /// <summary>
    /// Painter that aggregates one or two other painters. This makes it possible to:
    /// 1) Send equal drawing commands to more than one painter at a time
    /// 2) Override a specific attribute or function for an general painter object.
    /// 
    /// Getting attributes and querying function uses the mainpainter for this purpose.
    /// The subpainter can be null, in which case not used, otherwise all drawing
    /// commands are sent to this painter as well.
    /// </summary>
    public class PainterAggregate:Painter
    {
        public readonly Painter MainPainter;
        public readonly Painter SubPainter;

        public PainterAggregate(Painter mainpainter, Painter subpainter)
        {
            if (mainpainter == null)
                throw new Exception("mainpainter of PainterAggregate cannot be null");
            this.MainPainter = mainpainter;
            this.SubPainter = subpainter;
        }


        #region ATTRIBUTES

        public override int Color
        {
            get
            {
                return MainPainter.Color;
            }
            set
            {
                MainPainter.Color = value;
                if (SubPainter != null) SubPainter.Color = value;
            }
        }


        public override bool AntiGrain
        {
            get
            {
                return MainPainter.AntiGrain;
            }
            set
            {
                MainPainter.AntiGrain = value;
                if (SubPainter != null) SubPainter.AntiGrain = value;
            }
        }

        public override Hatch Hatch
        {
            get
            {
                return MainPainter.Hatch;
            }
            set
            {
                MainPainter.Hatch = value;
                if (SubPainter != null) SubPainter.Hatch = value;
            }
        }

        public override FillStyle FillStyle
        {
            get
            {
                return MainPainter.FillStyle;
            }
            set
            {
                MainPainter.FillStyle = value;
                if (SubPainter != null) SubPainter.FillStyle = value;
            }
        }

        public override double LineWidth
        {
            get
            {
                return MainPainter.LineWidth;
            }
            set
            {
                MainPainter.LineWidth = value;
                if (SubPainter != null) SubPainter.LineWidth = value;
            }
        }

        public override LineStyle LineStyle
        {
            get
            {
                return MainPainter.LineStyle;
            }
            set
            {
                MainPainter.LineStyle = value;
                if (SubPainter != null) SubPainter.LineStyle = value;
            }
        }

        public override double[] LineStyleDashes
        {
            get
            {
                return MainPainter.LineStyleDashes;
            }
            set
            {
                MainPainter.LineStyleDashes = value;
                if (SubPainter != null) SubPainter.LineStyleDashes = value;
            }
        }

        public override EndCap EndCaps
        {
            get
            {
                return MainPainter.EndCaps;
            }
            set
            {
                MainPainter.EndCaps = value;
                if (SubPainter != null) SubPainter.EndCaps = value;
            }
        }


        public override LineJoin LineJoin
        {
            get
            {
                return MainPainter.LineJoin;
            }
            set
            {
                MainPainter.LineJoin = value;
                if (SubPainter != null) SubPainter.LineJoin = value;
            }
        }

        public override Picture Pattern
        {
            get
            {
                return MainPainter.Pattern;
            }
            set
            {
                MainPainter.Pattern = value;
                if (SubPainter != null) SubPainter.Pattern = value;
            }
        }


        public override Transform2d Transform
        {
            get
            {
                return MainPainter.Transform;
            }
            set
            {
                MainPainter.Transform = value;
                if (SubPainter != null) SubPainter.Transform = value;
            }
        }

        public override Rectangle2i Clip
        {
            get
            {
                return MainPainter.Clip;
                
            }
            set
            {
                MainPainter.Clip = value;
                if (SubPainter != null) SubPainter.Clip = value;
            }
        }

        public override Transform2d PatternTransform
        {
            get
            {
                return MainPainter.PatternTransform;
            }
            set
            {
                MainPainter.PatternTransform = value;
                if (SubPainter != null) SubPainter.PatternTransform = value;
            }
        }

        public override FillMode FillMode
        {
            get
            {
                return MainPainter.FillMode;
            }
            set
            {
                MainPainter.FillMode = value;
                if (SubPainter != null) SubPainter.FillMode = value;
            }
        }


        public override double Opacity
        {
            get
            {
                return MainPainter.Opacity;
            }
            set
            {
                MainPainter.Opacity = value;
                if (SubPainter != null) SubPainter.Opacity = value;
            }
        }

        public override string Info
        {
            get { return "Aggregate painter"; }
        }

        public override void ResetAttributes()
        {
            MainPainter.ResetAttributes();
            if (SubPainter != null)
                SubPainter.ResetAttributes();
        }


        public override object Tag
        {
            get
            {
                return MainPainter.Tag;
            }
            set
            {

                //sets our own, and both secondary painters tags
                MainPainter.Tag = value;
                if (SubPainter != null) SubPainter.Tag = value;
                base.Tag = value;
            }
        }

        #endregion ATTRIBUTES

        #region LINE_PRIMITIVES

        public override void SetPixel(int x, int y, int rgb)
        {
            MainPainter.SetPixel(x, y, rgb);
            if (SubPainter != null) SubPainter.SetPixel(x, y, rgb);
        }

        public override int GetPixel(int x, int y)
        {
            return MainPainter.GetPixel(x, y);
        }

        public override void DrawRectangle(double x1, double y1, double x2, double y2)
        {
            MainPainter.DrawRectangle(x1, y1, x2, y2);
            if (SubPainter != null) SubPainter.DrawRectangle(x1, y1, x2, y2);
        }

        public override void DrawRectangle(int x1, int y1, int x2, int y2)
        {
            MainPainter.DrawRectangle(x1, y1, x2, y2);
            if (SubPainter != null) SubPainter.DrawRectangle(x1, y1, x2, y2);
        }

        public override void DrawRectangleT(double x1, double y1, double x2, double y2)
        {
            MainPainter.DrawRectangleT(x1, y1, x2, y2);
            if (SubPainter != null) SubPainter.DrawRectangleT(x1, y1, x2, y2);
        }


        public override void DrawLine(double x1, double y1, double x2, double y2)
        {
            MainPainter.DrawLine(x1, y1, x2, y2);
            if (SubPainter != null) SubPainter.DrawLine(x1, y1, x2, y2);
        }

        public override void DrawLine(int x1, int y1, int x2, int y2)
        {
            MainPainter.DrawLine(x1, y1, x2, y2);
            if (SubPainter != null) SubPainter.DrawLine(x1, y1, x2, y2);
        }

        public override void DrawLineT(double x1, double y1, double x2, double y2)
        {
            MainPainter.DrawLineT(x1, y1, x2, y2);
            if (SubPainter != null) SubPainter.DrawLineT(x1, y1, x2, y2);
        }

        public override void DrawCircle(double cx, double cy, double radius)
        {
            MainPainter.DrawCircle(cx, cy, radius);
            if (SubPainter != null) SubPainter.DrawCircle(cx, cy, radius);
        }

        public override void DrawCircle(int cx, int cy, int radius)
        {
            MainPainter.DrawCircle(cx, cy, radius);
            if (SubPainter != null) SubPainter.DrawCircle(cx, cy, radius);
        }

        public override void DrawCircleT(double cx, double cy, double radius)
        {
            MainPainter.DrawCircleT(cx, cy, radius);
            if (SubPainter != null) SubPainter.DrawCircleT(cx, cy, radius);
        }

        public override void DrawArc(double x1, double y1, double x2, double y2, double bulge)
        {
            MainPainter.DrawArc(x1, y1, x2, y2, bulge);
            if (SubPainter != null) SubPainter.DrawArc(x1, y1, x2, y2, bulge);
        }

        public override void DrawArc(int x1, int y1, int x2, int y2, double bulge)
        {
            MainPainter.DrawArc(x1, y1, x2, y2, bulge);
            if (SubPainter != null) SubPainter.DrawArc(x1, y1, x2, y2, bulge);
        }

        public override void DrawArcT(double x1, double y1, double x2, double y2, double bulge)
        {
            MainPainter.DrawArcT(x1, y1, x2, y2, bulge);
            if (SubPainter != null) SubPainter.DrawArcT(x1, y1, x2, y2, bulge);
        }

        public override void DrawEllipse(double cx, double cy, double aradius, double bradius, double tilt)
        {
            MainPainter.DrawEllipse(cx, cy, aradius, bradius, tilt);
            if (SubPainter != null) SubPainter.DrawEllipse(cx, cy, aradius, bradius, tilt);
        }

        public override void DrawEllipse(int cx, int cy, int aradius, int bradius, double tilt)
        {
            MainPainter.DrawEllipse(cx, cy, aradius, bradius, tilt);
            if (SubPainter != null) SubPainter.DrawEllipse(cx, cy, aradius, bradius, tilt);
        }

        public override void DrawEllipseT(double cx, double cy, double aradius, double bradius, double tilt)
        {
            MainPainter.DrawEllipseT(cx, cy, aradius, bradius, tilt);
            if (SubPainter != null) SubPainter.DrawEllipseT(cx, cy, aradius, bradius, tilt);
        }

        public override void DrawEllipticArc(double cx, double cy, double aradius, double bradius, double tilt, double startangle, double sweepangle)
        {
            MainPainter.DrawEllipticArc(cx, cy, aradius, bradius, tilt, startangle, sweepangle);
            if (SubPainter != null) SubPainter.DrawEllipticArc(cx, cy, aradius, bradius, tilt, startangle, sweepangle);
        }

        public override void DrawEllipticArc(int cx, int cy, int aradius, int bradius, double tilt, double startangle, double sweepangle)
        {
            MainPainter.DrawEllipticArc(cx, cy, aradius, bradius, tilt, startangle, sweepangle);
            if (SubPainter != null) SubPainter.DrawEllipticArc(cx, cy, aradius, bradius, tilt, startangle, sweepangle);
        }

        public override void DrawEllipticArcT(double cx, double cy, double aradius, double bradius, double tilt, double startangle, double sweepangle)
        {
            MainPainter.DrawEllipticArcT(cx, cy, aradius, bradius, tilt, startangle, sweepangle);
            if (SubPainter != null) SubPainter.DrawEllipticArcT(cx, cy, aradius, bradius, tilt, startangle, sweepangle);
        }

        public override void DrawPolyLine(bool close, params double[] xy)
        {
            MainPainter.DrawPolyLine(close, xy);
            if (SubPainter != null) SubPainter.DrawPolyLineT(close, xy);
        }

        public override void DrawPolyLineT(bool close, params double[] xy)
        {
            MainPainter.DrawPolyLineT(close, xy);
            if (SubPainter != null) SubPainter.DrawPolyLineT(close, xy);
        }


        public override void DrawPath(GFXPath path)
        {
            MainPainter.DrawPath(path);
            if (SubPainter != null) SubPainter.DrawPath(path);
        }

        public override void DrawPathT(GFXPath path)
        {
            MainPainter.DrawPathT(path);
            if (SubPainter != null) SubPainter.DrawPathT(path);
        }

        #endregion LINE_PRIMITIVES

        #region FILLED_PRIMITVES

        public override void FillRectangle(int x1, int y1, int x2, int y2)
        {
            MainPainter.FillRectangle(x1, y1, x2, y2);
            if (SubPainter != null) SubPainter.FillRectangle(x1, y1, x2, y2);
        }

        public override void FillRectangle(double x1, double y1, double x2, double y2)
        {
            MainPainter.FillRectangle(x1, y1, x2, y2);
            if (SubPainter != null) SubPainter.FillRectangle(x1, y1, x2, y2);
        }

        public override void FillRectangleT(double x1, double y1, double x2, double y2)
        {
            MainPainter.FillRectangleT(x1, y1, x2, y2);
            if (SubPainter != null) SubPainter.FillRectangleT(x1, y1, x2, y2);
        }

        public override void FillGradientRectangle(double x1, double y1, double x2, double y2, int y1color, int y2color)
        {
            MainPainter.FillGradientRectangle(x1, y1, x2, y2, y1color, y2color);
            if (SubPainter != null) SubPainter.FillGradientRectangle(x1, y1, x2, y2, y1color, y2color);
        }

        public override void FillGradientRectangle(int x1, int y1, int x2, int y2, int y1color, int y2color)
        {
            MainPainter.FillGradientRectangle(x1, y1, x2, y2, y1color, y2color);
            if (SubPainter != null) SubPainter.FillGradientRectangle(x1, y1, x2, y2, y1color, y2color);
        }

        public override void FillPath(GFXPath path)
        {
            MainPainter.FillPath(path);
            if (SubPainter != null) SubPainter.FillPath(path);
        }

        public override void FillPathT(GFXPath path)
        {
            MainPainter.FillPathT(path);
            if (SubPainter != null) SubPainter.FillPathT(path);
        }

        #endregion FILLED_PRIMITIVES

        #region TEXT

     

        public override void DrawText(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            MainPainter.DrawText(txt, x, y, size, angle, align);
            if (SubPainter != null) SubPainter.DrawText(txt, x, y, size, angle, align);
        }

        public override void DrawTextT(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            MainPainter.DrawTextT(txt, x, y, size, angle, align);
            if (SubPainter != null) SubPainter.DrawTextT(txt, x, y, size, angle, align);
        }

        public override void GetTextSize(string txt, double size, out double w, out double h)
        {
            MainPainter.GetTextSize(txt, size, out w, out h);
        }

        public override double[] GetTextBounds(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            return MainPainter.GetTextBounds(txt, x, y, size, angle, align);
        }

        public override double[] GetTextBox(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            return MainPainter.GetTextBox(txt, x, y, size, angle, align);
        }

        public override GFXPath GetTextPath(string txt, double x, double y, double size, double angle, TextAlign align, Transform2d t)
        {
            return MainPainter.GetTextPath(txt, x, y, size, angle, align, t);
        }

        public override void GetFontDim(double size, out double linespace_pixels, out double ascent_pixels, out double descent_pixels,out bool filled)
        {
            MainPainter.GetFontDim(size, out linespace_pixels, out ascent_pixels, out descent_pixels,out filled);
        }

        #endregion TEXT

        #region IMAGES


        public override void DrawPicture(Picture p, double x, double y)
        {
            MainPainter.DrawPicture(p, x, y);
            if (SubPainter != null) SubPainter.DrawPicture(p, x, y);
        }

        public override void DrawPictureT(Picture p, double x, double y)
        {
            MainPainter.DrawPictureT(p, x, y);
            if (SubPainter != null) SubPainter.DrawPictureT(p, x, y);
        }

        /*public override void Blit(Picture p, int dest_x1, int dest_y1, int dest_x2, int dest_y2, int src_x, int src_y)
        {
            MainPainter.Blit(p, dest_x1, dest_y1, dest_x2, dest_y2, src_x, src_y);
            if (SubPainter != null) SubPainter.Blit(p, dest_x1, dest_y1, dest_x2, dest_y2, src_x, src_y);
        }*/

        public override void Blit(Picture p, int dst_x, int dst_y, int dst_w, int dst_h, int src_x, int src_y, int src_w, int src_h)
        {
            MainPainter.Blit(p, dst_x, dst_y, dst_w, dst_h, src_x, src_y, src_w, src_h);
            if (SubPainter != null) SubPainter.Blit(p, dst_x, dst_y, dst_w, dst_h, src_x, src_h, src_w, src_h);
        }

        public override Picture CreatePictureFromFile(string filename)
        {
            return MainPainter.CreatePictureFromFile(filename);
        }

        public override Picture CreatePictureFromResource(string resname)
        {
            return MainPainter.CreatePictureFromResource(resname);
        }

        public override Picture CreatePictureFromResource(string resname, System.Reflection.Assembly asm)
        {
            return MainPainter.CreatePictureFromResource(resname, asm);
        }

        public override Picture CreatePictureFromSize(int width, int height)
        {
            return MainPainter.CreatePictureFromSize(width, height);
        }

        public override Picture CreatePictureFromStream(Stream str)
        {
            return MainPainter.CreatePictureFromStream(str);
        }

        public override Picture CreatePictureFromSoftPicture(SoftPicture img)
        {
            return MainPainter.CreatePictureFromSoftPicture(img);
        }

        public override Picture MakeCompatiblePicture(Picture p)
        {
            return MainPainter.MakeCompatiblePicture(p);
        }


        public override void CopyPicture(ref Picture pic, int x1, int y1, int x2, int y2)
        {
            MainPainter.CopyPicture(ref pic, x1, y1, x2, y2);
        }

        #endregion IMAGES

        #region MARKS


        public override void DrawMark(int x, int y, MarkType type, int size)
        {
            MainPainter.DrawMark(x, y, type, size);
            if (SubPainter != null) SubPainter.DrawMark(x, y, type, size);
        }
        
        public override void DrawMark(double x, double y, MarkType type, int size)
        {
            MainPainter.DrawMark(x, y, type, size);
            if (SubPainter != null) SubPainter.DrawMark(x, y, type, size);
        }

        public override void DrawMarkT(double x, double y, MarkType type, int size)
        {
            MainPainter.DrawMarkT(x, y, type, size);
            if (SubPainter != null) SubPainter.DrawMarkT(x, y, type, size);
        }

        #endregion MARKS

        #region MISC

        public override void Flush()
        {
            MainPainter.Flush();
            if (SubPainter != null) SubPainter.Flush();
        }

        public override void Clear(int rgb)
        {
            MainPainter.Clear(rgb);
            if (SubPainter != null) SubPainter.Clear(rgb);
        }

        public override int Width
        {
            get { return MainPainter.Width; }
        }


        public override int Height
        {
            get { return MainPainter.Height; }
        }

        public override void MillimetersToPixels(double mmx, double mmy, out double pixx, out double pixy)
        {
            MainPainter.MillimetersToPixels(mmx, mmy, out pixx, out pixy);
        }

        public override void PixelsToMillimeters(double pixx, double pixy, out double mmx, out double mmy)
        {
            MainPainter.PixelsToMillimeters(pixx, pixy, out mmx, out mmy);
        }

        public override GraphicAttributes Save()
        {
            return MainPainter.Save();
        }

        public override void Restore(GraphicAttributes ga)
        {
            MainPainter.Restore(ga);
            if (SubPainter != null)
                SubPainter.Restore(ga);
        }


        public override void Dispose()
        {
            //do nothing, callers should dispose MainPainter and SubPainter themselves
        }

        #endregion MISC
    }
}
