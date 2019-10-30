using Guppy2.Calc.Geom2d;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace Guppy2.GFX
{
    public class PainterExtentsRecorder:Painter
    {
        public Box2d RecordedExtents = new Box2d();
        

        public PainterExtentsRecorder()
        {
            
        }

        public override string Info
        {
            get { return "Extents recorder";}
        }



        #region UTIL
        
        private void AppendPathToExtents(bool uselinewidth, bool usetransform, GFXPath pth)
        {
            Box2d ext;

            if (usetransform)
                ext = pth.GetTransformedExtents(Transform);
            else
                ext = pth.Extents;


            if (uselinewidth)
                AppendWithLineWidth(pth.Extents);
            else
                RecordedExtents.Append(ext.XMin, ext.YMin, ext.XMax, ext.YMax);

        }

        #endregion

        #region LINE_PRIMITIVES

        public override void SetPixel(int x, int y, int rgb)
        {
            RecordedExtents.Append(x, y);
        }

        public override int GetPixel(int x, int y)
        {
            return 0;
        }

        public override void DrawLine(double x1, double y1, double x2, double y2)
        {
            AppendWithLineWidth(x1, y1, x2, y2);
        }

        public override void DrawEllipticArc(double cx, double cy, double aradius, double bradius, double tilt, double startangle, double sweepangle)
        {
            var ellipext = GeomUtil.GetEllipticExtents(cx, cy, aradius, bradius, startangle, sweepangle, tilt);
            AppendWithLineWidth(ellipext);
        }

        public override void DrawCircle(double cx, double cy, double radius)
        {
            AppendWithLineWidth(cx - radius, cy - radius, cx + radius, cy + radius);
        }


        public override void DrawArc(double x1, double y1, double x2, double y2, double bulge)
        {
            Box2d rect=GeomUtil.GetArcExtents(x1, y1, x2, y2, bulge);
            AppendWithLineWidth(rect);
        }

        public override void DrawPolyLine(bool close, params double[] xy)
        {
            AppendXY(xy, true);
        }

        public override void DrawPath(GFXPath p)
        {
            AppendPathToExtents(false, false, p);
        }

        public override void DrawPathT(GFXPath p)
        {
            AppendPathToExtents(false, true, p);
        }

        #endregion

        private void AppendXY(double[] xy, bool uselinewidth)
        {
            int numpts = xy.Length >> 1;
            if (numpts > 0)
            {
                int xyidx = 0;
                double xmin = xy[0];
                double ymin = xy[1];
                double xmax = xmin;
                double ymax = ymin;


                for (int idx = 0; idx < numpts; idx++)
                {
                    double x = xy[xyidx++];
                    double y = xy[xyidx++];
                    if (x < xmin) xmin = x;
                    if (x > xmax) xmax = x;
                    if (y < ymin) ymin = y;
                    if (y > ymax) ymax = y;
                }

                if (uselinewidth)
                    AppendWithLineWidth(xmin, ymin, xmax, ymax);
                else
                    RecordedExtents.Append(xmin, ymin, xmax, ymax);
            }
        }

   
        private void AppendWithLineWidth(double xmin, double ymin, double xmax, double ymax)
        {
            
            double ofs=LineWidth/2.0;
            if (EndCaps == EndCap.Square)
                ofs *= Math.Sqrt(2); //worst case for 45 degree line
            
            xmin-=ofs;
            ymin-=ofs;
            xmax+=ofs;
            ymax+=ofs;

            RecordedExtents.Append(xmin,ymin,xmax,ymax);
        }

        private void AppendWithLineWidth(Box2d rect)
        {
            AppendWithLineWidth(rect.XMin, rect.YMin, rect.XMax, rect.YMax);
        }


        #region FILLED_PRIMITIVES

        public override void FillGradientRectangle(double x1, double y1, double x2, double y2, int y1color, int y2color)
        {
            RecordedExtents.Append(x1, y1, x2, y2);
        }

        public override void FillPath(GFXPath p)
        {
            AppendPathToExtents(false, false, p);
        }

        public override void FillPathT(GFXPath p)
        {
            AppendPathToExtents(false, true, p);
        }


        #endregion

       
        public override void DrawText(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            double[] bounds = GetTextBounds(txt, x, y, size, angle, align);
            RecordedExtents.Append(bounds[0], bounds[1]);
            RecordedExtents.Append(bounds[2], bounds[3]);
            RecordedExtents.Append(bounds[4], bounds[5]);
            RecordedExtents.Append(bounds[6], bounds[7]);
        }

        public override void DrawTextT(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            double[] bounds = GetTextBounds(txt, x, y, size, angle, align);

            Transform.Apply(bounds[0], bounds[1], out bounds[0], out bounds[1],true);
            Transform.Apply(bounds[2], bounds[3], out bounds[2], out bounds[3], true);
            Transform.Apply(bounds[4], bounds[5], out bounds[4], out bounds[5], true);
            Transform.Apply(bounds[6], bounds[7], out bounds[6], out bounds[7], true);

            RecordedExtents.Append(bounds[0], bounds[1]);
            RecordedExtents.Append(bounds[2], bounds[3]);
            RecordedExtents.Append(bounds[4], bounds[5]);
            RecordedExtents.Append(bounds[6], bounds[7]);
        }


        public override double[] GetTextBounds(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            return null;    //not in this painter
        }

        public override void GetTextSize(string txt, double size, out double w, out double h)
        {
            w = h = 0.0;    //not in this painter
        }

        public override void GetFontDim(double size, out double linespace_pixels, out double ascent_pixels, out double descent_pixels,out bool filled)
        {
            linespace_pixels = size * 1.5; //not really supported, use somewhat reasonable values
            ascent_pixels = size * 0.8;
            descent_pixels = size * 0.2;
            filled = false;
        }


        public override Picture CreatePictureFromSize(int width, int height)
        {
            return null; //not in this painter
        }

        public override Picture CreatePictureFromStream(System.IO.Stream str)
        {
            return null;  //not in this painter
        }

        public override Picture CreatePictureFromSoftPicture(SoftPicture img)
        {
            return null;  //not in this painter
        }

        public override Picture MakeCompatiblePicture(Picture p)
        {
            return p;
        }

        public override GFXPath GetTextPath(string txt, double x, double y, double size, double angle, TextAlign align, Transform2d t)
        {
            return null;    //not in this painter
        }

        public override void CopyPicture(ref Picture pic, int x1, int y1, int x2, int y2)
        {
            pic = null; //not in this painter
        }

        public override void DrawPicture(Picture p, double x, double y)
        {
            RecordedExtents.Append(x, y, x + p.Width, y + p.Height);
        }

        public override void DrawPictureT(Picture p, double x, double y)
        {
            DrawRectangleT(x, y, x + p.Width, y + p.Height); //TODO: uses line width, and FillRectangleT in base class is slow. Fix this. 
        }

        public override void Blit(Picture p, int dst_x, int dst_y, int dst_w, int dst_h, int src_x, int src_y, int src_w, int src_h)
        {
            RecordedExtents.Append(dst_x,dst_y,dst_x+dst_w, dst_y+dst_h);
        }


        public override void Clear(int rgb)
        {
            RecordedExtents = new Box2d();
        }

        public override int Width
        {
            get { return (int)Math.Ceiling(RecordedExtents.Width); }
        }

        public override int Height
        {
            get { return (int)Math.Ceiling(RecordedExtents.Height); }
        }

        public override void MillimetersToPixels(double mmx, double mmy, out double pixx, out double pixy)
        {
            pixx = mmx;
            pixy = mmy;
        }

        public override void Dispose()
        {
            
        }
        
    }
}
