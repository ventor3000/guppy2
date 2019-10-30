using Guppy2.Calc.Geom2d;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace Guppy2.GFX
{
    public class PainterFeedback:Painter
    {
        double[] clipbuffer = new double[50]; //used for circle and arc clippint

        double flattentol = 0.5;
        PictureFeedback target;
        object currentobject;
        Painter measurepainter = null;  //created on demand
        
        public PainterFeedback(PictureFeedback target,Painter measurepainter)
        {
            this.target = target;
            base.Clip = new Rectangle2i(0, 0, target.Width - 1, target.Height - 1);
        }

        #region MISC

        public override string Info
        {
            get { return "Feedback painter"; }
        }

        public virtual object CurrentObject
        {
            get
            {
                return currentobject;
            }
            set
            {
                currentobject = value;
            }
        }

        public override void Clear(int rgb)
        {
            target.Clear();
        }

        public override void Dispose()
        {
            if (measurepainter != null)
            {
                measurepainter.Dispose();
                measurepainter = null;
            }
        }

        Painter MeasurePainter
        {
            get
            {

                return measurepainter;
                /*if (measurepainter != null)
                    return measurepainter;
                
                measurepainter = new PainterGDIPlus(true);
                return measurepainter;*/
            }
        }

        public override int Height
        {
            get { return target.Height; }
        }

        public override int Width
        {
            get { return target.Width; }
        }

        public override void MillimetersToPixels(double mmx, double mmy, out double pixx, out double pixy)
        {
            pixx = mmx;
            pixy = mmy;
        }
        
        #endregion

        #region LINE_PRIMITVES

        public override void DrawLine(double x1, double y1, double x2, double y2)
        {
            if(Clipper.ClipLine(ref x1,ref y1,ref x2,ref y2,Clip.XMin,Clip.YMin,Clip.XMax,Clip.YMax))
                target.Add(GFXUtil.FloatToInt(x1), GFXUtil.FloatToInt(y1), GFXUtil.FloatToInt(x2), GFXUtil.FloatToInt(y2),currentobject);
        }


        public override void DrawArc(double x1, double y1, double x2, double y2, double bulge)
        {
            int prevx=GFXUtil.FloatToInt(x1);
            int prevy=GFXUtil.FloatToInt(y1);

            int clipres = Clipper.ClipArc(x1, y1, x2, y2, bulge, clipbuffer, Clip.XMin,Clip.YMin,Clip.XMax,Clip.YMax);

            for (int idx = 0; idx < clipres; idx += 5)
            {
                int cx = GFXUtil.FloatToInt(clipbuffer[idx]), cy = GFXUtil.FloatToInt(clipbuffer[idx + 1]), nx, ny;
                GeomUtil.FlattenArc(clipbuffer[idx], clipbuffer[idx + 1], clipbuffer[idx + 2], clipbuffer[idx + 3], clipbuffer[idx + 4], false, flattentol, (x, y, moveto) =>
                {
                    nx = GFXUtil.FloatToInt(x);
                    ny = GFXUtil.FloatToInt(y);
                    target.Add(cx,cy,nx,ny,currentobject);
                    cx = nx;
                    cy = ny;
                });
            }
        }

        public override void FillRectangle(double x1, double y1, double x2, double y2)
        {
            int ix1=GFXUtil.FloatToInt(x1);
            int iy1=GFXUtil.FloatToInt(y1);
            int ix2=GFXUtil.FloatToInt(x2);
            int iy2=GFXUtil.FloatToInt(y2);
            DrawLine(ix1, iy1, ix2, iy1);
            DrawLine(ix2, iy1, ix2, iy2);
            DrawLine(ix2, iy2, ix1, iy2);
            DrawLine(ix1, iy2, ix1, iy1);
        }

        public override void DrawCircle(double cx, double cy, double radius)
        {
            DrawArc(cx + radius, cy, cx - radius, cy, 1.0);
            DrawArc(cx - radius, cy, cx + radius, cy, 1.0);
        }

        public override void SetPixel(int x, int y, int rgb)
        {
            if(x >= 0 && y >= 0 && x < Width && y < Height)
                target.Add(x, y, x, y, currentobject);
        }

        public override int GetPixel(int x, int y)
        {
            return 0;
        }

        public override void DrawEllipticArc(double cx, double cy, double aradius, double bradius, double tilt, double startangle, double sweepangle)
        {
          /*  int prevx=0,prevy=0;
            int nextx,nexty;
            bool first=true;

            GeomUtil.FlattenEllipticArc(cx, cy, aradius, bradius, tilt, startangle, sweepangle, flattentol, true, (x, y,moveto) =>
            {
                if (first)
                {
                    prevx = GFXUtil.RealToInt(x);
                    prevy = GFXUtil.RealToInt(y);
                    first = false;
                }
                else
                {
                    nextx = GFXUtil.RealToInt(x);
                    nexty = GFXUtil.RealToInt(y);
                    DrawLine(prevx,prevy,nextx,nexty);
                    //target.Add(prevx, prevy, nextx, nexty, currentobject);
                    prevx = nextx;
                    prevy = nexty;
                }
            });*/

            int px = 0, py = 0, nx, ny;

            int num = Clipper.ClipEllipticArc(cx, cy, aradius, bradius, tilt, startangle, sweepangle, clipbuffer, Clip.XMin,Clip.YMin,Clip.XMax,Clip.YMax);

            for (int l = 0; l < num; l += 2)
            {
                GeomUtil.FlattenEllipticArc(cx, cy, aradius, bradius, tilt, clipbuffer[l], clipbuffer[l + 1] - clipbuffer[l], flattentol, true, (x, y, moveto) =>
                {
                    nx = GFXUtil.FloatToInt(x);
                    ny = GFXUtil.FloatToInt(y);
                    if (!moveto)
                        DrawLine(px, py, nx, ny); 
                    px = nx;
                    py = ny;
                });
            }
        }

        public override void DrawPolyLine(bool close, params double[] xy)
        {

            if (xy.Length < 1)
                return;

            int prevx = GFXUtil.FloatToInt(xy[0]);
            int prevy = GFXUtil.FloatToInt(xy[1]);
            int startx = prevx, starty = prevy;
            int nextx, nexty;

            if (xy.Length == 1)
            {
                DrawLine(prevx, prevy, prevx, prevy);
                return;
            }

            for (int l = 2; l < xy.Length; l+=2)
            {
                nextx = GFXUtil.FloatToInt(xy[l]);
                nexty = GFXUtil.FloatToInt(xy[l + 1]);
                DrawLine(prevx, prevy, nextx, nexty);
                prevx = nextx;
                prevy = nexty;
            }

            if (close)
                DrawLine(prevx, prevy, startx, starty);
        }

        #endregion


        #region FILLED_PRIMITIVES

        public override void FillRectangle(int x1, int y1, int x2, int y2)
        {
            DrawRectangle(x1, y1, x2, y2);
        }
        
        public override void FillGradientRectangle(double x1, double y1, double x2, double y2, int y1color, int y2color)
        {
            DrawRectangle(x1, y1, x2, y2);
        }



        #endregion


        #region PATH

        

   


        private Matrix TransformToMatrix(Transform2d tm)
        {
            return new Matrix((float)tm.AX, (float)tm.AY, (float)tm.BX, (float)tm.BY, (float)tm.TX, (float)tm.TY);
        }

        /// <summary>
        /// Strokes a path using GDI
        /// </summary>
        /// <param name="trans"></param>
        public void InternalStrokePath(GFXPath path,Transform2d trans,bool forceclosed,bool allowmodifypath)
        {
            int cx=0,cy=0,nx,ny;

            if (trans == null || trans.IsIdentity)
            {
                path.Flatten(flattentol, (x, y, moveto) =>
                {
                    if (moveto)
                    {
                        cx = GFXUtil.FloatToInt(x);
                        cy = GFXUtil.FloatToInt(y);
                    }
                    else
                    {
                        nx = GFXUtil.FloatToInt(x);
                        ny = GFXUtil.FloatToInt(y);
                        DrawLine(nx,ny,cx,cy);
                        //target.Add(nx, ny, cx, cy, currentobject);
                        cx = nx;
                        cy = ny;
                    }
                });
            }
            else
            {
                path.TransformFlatten(flattentol,trans, (x, y, moveto) =>
                {
                    if (moveto)
                    {
                        cx = GFXUtil.FloatToInt(x);
                        cy = GFXUtil.FloatToInt(y);
                    }
                    else
                    {
                        nx = GFXUtil.FloatToInt(x);
                        ny = GFXUtil.FloatToInt(y);
                        //target.Add(nx, ny, cx, cy, currentobject);
                        DrawLine(nx, ny, cx, cy);
                        cx = nx;
                        cy = ny;
                    }
                });
            }
        
        }

        private void InternalDrawBezier(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            int n = Clipper.ClipBezier(x1, y1, x2, y2, x3, y3, x4, y4, clipbuffer, Clip.XMin, Clip.YMin, Clip.XMax, Clip.YMax);
            int penx = 0, peny = 0;

            for (int l = 0; l < n; l += 2)
            {
                GeomUtil.FlattenBezier(x1, y1, x2, y2, x3, y3, x4, y4, true, flattentol, (x, y, moveto) =>
                {
                    int nextx = GFXUtil.FloatToInt(x);
                    int nexty = GFXUtil.FloatToInt(y);

                    if (!moveto)
                        DrawLine(penx, peny, nextx, nexty);

                    penx = nextx;
                    peny = nexty;

                }, clipbuffer[l], clipbuffer[l + 1]);
            }
        }


        private void InternalDrawBezierT(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            Transform.Apply(x1, y1, out x1, out y1, true);
            Transform.Apply(x2, y2, out x2, out y2, true);
            Transform.Apply(x3, y3, out x3, out y3, true);
            Transform.Apply(x4, y4, out x4, out y4, true);

            InternalDrawBezier(x1, y1, x2, y2, x3, y3, x4, y4);
        }



        public override void FillPath(GFXPath path)
        {
            InternalStrokePath(path, null, true, false);
        }

        public override void FillPathT(GFXPath path)
        {
            InternalStrokePath(path, Transform, true, false);
        }

        public override void DrawPath(GFXPath path)
        {
            //InternalStrokePath(path, null, false, false);

            path.EnumerateSegments(
               (x1, y1, x2, y2) =>
               { //line
                   DrawLine(x1, y1, x2, y2);
               },
               (x1, y1, x2, y2, bulge) =>
               { //arc
                   DrawArc(x1, y1, x2, y2, bulge);
               },
               (x1, y1, x2, y2, x3, y3, x4, y4) =>
               { //bezier
                   InternalDrawBezier(x1, y1, x2, y2, x3, y3, x4, y4);
               }
           );
        }

        public override void DrawPathT(GFXPath path)
        {
            //InternalStrokePath(path, Transform, false, false);

            path.EnumerateSegments(
                (x1, y1, x2, y2) =>
                { //line
                    DrawLineT(x1, y1, x2, y2);
                },
                (x1, y1, x2, y2, bulge) =>
                { //arc
                    DrawArcT(x1, y1, x2, y2, bulge);
                },
                (x1, y1, x2, y2, x3, y3, x4, y4) =>
                { //bezier
                    InternalDrawBezierT(x1, y1, x2, y2, x3, y3, x4, y4);
                }
            );
        }

      

        #endregion


        #region IMAGES
        
        public override void CopyPicture(ref Picture pic, int x1, int y1, int x2, int y2)
        {
            //Do nothing in this painter    
        }

        public override void DrawPicture(Picture p, double x, double y)
        {
            DrawRectangle(x, y, x + p.Width-1, y + p.Height-1);
        }


        public override void DrawPictureT(Picture p, double x, double y)
        {
            DrawRectangleT(x, y, x + p.Width - 1, y + p.Height - 1);
        }

        public override Picture CreatePictureFromFile(string filename)
        {
            return null; //not in this painter!
        }

        public override Picture CreatePictureFromResource(string resname)
        {
            return null; //not in this painter!
        }

        public override Picture CreatePictureFromSize(int width, int height)
        {
            return null; //not in this painter!
        }

        public override Picture CreatePictureFromSoftPicture(SoftPicture img)
        {
            return null; //not in this painter!
        }

        public override Picture CreatePictureFromStream(System.IO.Stream str)
        {
            return null; //not in this painter!
        }

        public override Picture MakeCompatiblePicture(Picture p)
        {
            return null; //not in this painter!
        }

        public override void Blit(Picture p, int dst_x, int dst_y, int dst_w, int dst_h, int src_x, int src_y, int src_w, int src_h)
        {
            DrawRectangle(dst_x, dst_y, dst_w, dst_h);
        }

        #endregion

        #region TEXT

        
        public override void DrawText(string txt, double x, double y, double size, double angle, TextAlign align)
        {

            if (txt != null)
            {
                GFXPath pth = MeasurePainter.GetTextPath(txt, x, y, size, angle, align, null);
                if (pth == null)
                    return;
                InternalStrokePath(pth, null, false, true);
            }
        }

        public override void DrawTextT(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            if (txt != null)
            {
                GFXPath pth = MeasurePainter.GetTextPath(txt, x, y, size, angle, align, Transform);
                if (pth == null)
                    return;

                InternalStrokePath(pth, null, false, true);
            }
            
        }

        public override void GetTextSize(string txt, double size, out double w, out double h)
        {
            MeasurePainter.GetTextSize(txt, size, out w, out h);
        }

        public override GFXPath GetTextPath(string txt, double x, double y, double size, double angle, TextAlign align, Transform2d t)
        {
            return MeasurePainter.GetTextPath(txt, x, y, size, angle, align, t);
        }


        public override double[] GetTextBounds(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            return MeasurePainter.GetTextBounds(txt, x, y, size, angle, align);
        }

        public override void GetFontDim(double size, out double linespace_pixels, out double ascent_pixels, out double descent_pixels,out bool filled)
        {
            MeasurePainter.GetFontDim(size, out linespace_pixels, out ascent_pixels, out descent_pixels,out filled);
        }
        
        #endregion
    }
}

