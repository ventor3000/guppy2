using Guppy2.Calc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GFX.Windows
{

    //TODO: implement thin versions of missing primitives (circle, ellipse, elliptic arc...)
    
    public unsafe class PainterWinRGB:PainterGDIPlus
    {
        [Flags]
        private enum ThinLineModeFlag
        {
            OwnThinLines=0,  //if no other bits are set, sw can draw our own thin lines
            AntiGrain=1,    //if antialiasing is turned on we forward calls
            ThickLines=2,   //if lines are thick we forward calls
            Alpha=4,        //if alpha is on we forward calls
            CustomLineStyle=8, //custom linestyles are forwarded
            Opacity=16      //transparent
        }

        ThinLineModeFlag thinlinemode = ThinLineModeFlag.OwnThinLines; //default settings allows us to draw our own thin lines
        private int color;  //current rgb color, for speedy access we keep it locally
        public int*[] scanlines; //shortcut to targets scanlines
        PictureWinRGB target;
        double flattentol = 0.5;
        ushort stipplepattern;    //pattern used for stippled lines
        ushort stipplebit;        //current position in pattern, set as a single bit


        static ushort stipple_dot =             (ushort)Convert.ToInt32("1111000011110000", 2);
        static ushort stipple_dashdot =         (ushort)Convert.ToInt32("1111111100011000", 2);
        static ushort stipple_dashdotdot =      (ushort)Convert.ToInt32("1111111001101100", 2);
        static ushort stipple_dash =            (ushort)Convert.ToInt32("1111111100000000", 2);

        void UpdateLineMode(ThinLineModeFlag flag, bool on)
        {
            if (on)
                thinlinemode |= flag;
            else
                thinlinemode &= ~(flag);
        }
        
        public PainterWinRGB(PictureWinRGB pic)
            : base(pic)
        {
            this.target = pic;
            scanlines = pic.scanlines;
            color = (int)Color;
        }
        
        #region ATTRIBUTES

        //NOTE: we override attributes that affects our possibillity to draw our own thin lines.
        //Hard cases we forward to base GDIPlus driver, but one pixel wide standard lines we can do much faster ourselves.
        
        public override bool AntiGrain
        {
            get
            {
                return base.AntiGrain;
            }
            set
            {
                UpdateLineMode(ThinLineModeFlag.AntiGrain, value);
                base.AntiGrain = value;
            }
        }

        public override int Color
        {
            get
            {
                return base.Color;
            }
            set
            {
                base.Color = value;
                
                this.color = value; //store in local var. for speedy access
                UpdateLineMode(ThinLineModeFlag.Alpha, RGB.GetAlpha(value) > 0);
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
                base.Opacity = value;
                UpdateLineMode(ThinLineModeFlag.Opacity, Opacity < 1.0-MathUtil.Epsilon);
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
                base.LineWidth = value;
                UpdateLineMode(ThinLineModeFlag.ThickLines, value >= 1.5);
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
                
                base.LineStyle = value;
                UpdateLineMode(ThinLineModeFlag.CustomLineStyle, value == Guppy2.GFX.LineStyle.Custom);
                
                switch (value)
                {
                    
                    case Guppy2.GFX.LineStyle.Continuous: stipplepattern = 1; break; //never used since continuous lines are drawn with function that dont use stipplepattern
                    case Guppy2.GFX.LineStyle.Dash: stipplepattern = stipple_dash; break;
                    case Guppy2.GFX.LineStyle.DashDot: stipplepattern = stipple_dashdot; break;
                    case Guppy2.GFX.LineStyle.DashDotDot: stipplepattern = stipple_dashdotdot; break;
                    case Guppy2.GFX.LineStyle.Dot: stipplepattern = stipple_dot; break;
                    case Guppy2.GFX.LineStyle.Custom: stipplepattern = 1; break; //never used since custom linestyles falls back to GDI+ drawing
                }
            }
        }

        public override string Info
        {
            get
            {
                return "Windows RGB Painter";
            }
        }

        #endregion


        #region LINE_PRIMITIVES

        public override void SetPixel(int x, int y, int rgb)
        {
            target.SetPixel(x, y, rgb);
        }
        
        public override int GetPixel(int x, int y)
        {
            return target.GetPixel(x, y);
        }
        
        public override void DrawLine(double x1, double y1, double x2, double y2)
        {
           
            if (thinlinemode != ThinLineModeFlag.OwnThinLines)
            {
                base.DrawLine(x1, y1, x2, y2);
                return;
            }

            stipplebit = 0x8000;
            Line_Internal(GFXUtil.FloatToInt(x1), GFXUtil.FloatToInt(y1), GFXUtil.FloatToInt(x2), GFXUtil.FloatToInt(y2),true);
        }
        
        void Line_Internal(int x1, int y1, int x2, int y2,bool setlastpixel)
        {
            if (LineStyle != LineStyle.Continuous)
                Line_ThinStippled(x1, y1, x2, y2);
            else
                Line_ThinContinuous(x1, y1, x2, y2, setlastpixel);
        }

        private void Line_ThinContinuous(int x1, int y1, int x2, int y2, bool setlastpixel)
        {
            if (!Clipper.ClipLine(ref x1, ref y1, ref x2, ref y2, Clip.XMin,Clip.YMin,Clip.XMax,Clip.YMax))
                return;

            int d, x, y, ax, ay, sx, sy, dx, dy;
            dx = x2 - x1;
            ax = Math.Abs(dx) << 1;
            sx = dx < 0 ? -1 : 1;
            dy = y2 - y1;
            ay = Math.Abs(dy) << 1;
            sy = dy < 0 ? -1 : 1;
            x = x1;
            y = y1;
            if (ax > ay)
            {
                d = ay - (ax >> 1);
                while (x != x2)
                {
                    *(scanlines[y] + x) = color; //set pixel
                    if (d >= 0)
                    {
                        y += sy;
                        d -= ax;
                    }
                    x += sx;
                    d += ay;
                }
            }
            else
            {
                d = ax - (ay >> 1);
                while (y != y2)
                {
                    *(scanlines[y] + x) = color; //set pixel
                    if (d >= 0)
                    {
                        x += sx;
                        d -= ay;
                    }
                    y += sy;
                    d += ax;
                }
            }

            if (setlastpixel)
                *(scanlines[y] + x) = color; //set last pixel
        }

        void Line_ThinStippled(int x1, int y1, int x2, int y2)
        {

            if (!Clipper.ClipLine(ref x1, ref y1, ref x2, ref y2, Clip.XMin, Clip.YMin, Clip.XMax, Clip.YMax))
                return;

            int d, x, y, ax, ay, sx, sy, dx, dy;
            dx = x2 - x1;
            ax = Math.Abs(dx) << 1;
            sx = dx < 0 ? -1 : 1;
            dy = y2 - y1;
            ay = Math.Abs(dy) << 1;
            sy = dy < 0 ? -1 : 1;
            x = x1;
            y = y1;
            if (ax > ay)
            {
                d = ay - (ax >> 1);
                while (x != x2)
                {
                    ///Setpixel logic////////////////////////
                    if ((stipplepattern & stipplebit) != 0)
                        *(scanlines[y] + x) = color;
                    stipplebit >>= 1;
                    if (stipplebit == 0)
                        stipplebit = 0x8000;
                    ////////////////////////////////////////

                    if (d >= 0)
                    {
                        y += sy;
                        d -= ax;
                    }
                    x += sx;
                    d += ay;
                }
            }
            else
            {
                d = ax - (ay >> 1);
                while (y != y2)
                {
                    ///Setpixel logic////////////////////////
                    if ((stipplepattern & stipplebit) != 0)
                        *(scanlines[y] + x) = color;
                    stipplebit >>= 1;
                    if (stipplebit == 0)
                        stipplebit = 0x8000;
                    ////////////////////////////////////////
                    if (d >= 0)
                    {
                        x += sx;
                        d -= ay;
                    }
                    y += sy;
                    d += ax;
                }
            }

            ///Setpixel logic////////////////////////
            if ((stipplepattern & stipplebit) != 0)
                *(scanlines[y] + x) = color;
            stipplebit >>= 1;
            if (stipplebit == 0)
                stipplebit = 0x8000;
            ////////////////////////////////////////
        }



        public override void DrawRectangle(double x1, double y1, double x2, double y2)
        {
            if (thinlinemode != ThinLineModeFlag.OwnThinLines)
            {
                base.DrawRectangle(x1, y1, x2, y2);
                return;
            }
            
            stipplebit = 0x8000;

            int ix1 = GFXUtil.FloatToInt(x1);
            int iy1 = GFXUtil.FloatToInt(y1);
            int ix2 = GFXUtil.FloatToInt(x2);
            int iy2 = GFXUtil.FloatToInt(y2);

            if (ix1 == ix2)
            {
                if (iy1 == iy2)
                {
                    SetPixel(ix1, iy1,color);
                    return;
                }
                else {
                    Line_Internal(ix1, iy1, ix1, iy2,true); //vertical line
                    return;
                }
            }
            else if (iy1 == iy2)
            {
                Line_Internal(ix1, iy1, ix2, iy1, true); //horizontal line
                return;
            }

            Line_Internal(ix1, iy1, ix2, iy1,false);
            Line_Internal(ix2, iy1, ix2, iy2, false);
            Line_Internal(ix2, iy2, ix1, iy2, false);
            Line_Internal(ix1, iy2, ix1, iy1, false);
        }

        
        public override void DrawRectangleT(double x1, double y1, double x2, double y2)
        {
            if (thinlinemode != ThinLineModeFlag.OwnThinLines)
            {
                base.DrawRectangleT(x1, y1, x2, y2);
                return;
            }

            stipplebit = 0x8000;

            //convert to four points as poly to be able to transform
            double x3 = x2;
            double y3 = y2;
            y2 = y1;
            double x4 = x1;
            double y4 = y3;

            Transform.Apply(x1, y1, out x1, out y1,true);
            Transform.Apply(x2, y2, out x2, out y2,true);
            Transform.Apply(x3, y3, out x3, out y3, true);
            Transform.Apply(x4, y4, out x4, out y4, true);

            int ix1 = GFXUtil.FloatToInt(x1);
            int iy1 = GFXUtil.FloatToInt(y1);
            int ix2 = GFXUtil.FloatToInt(x2);
            int iy2 = GFXUtil.FloatToInt(y2);
            int ix3 = GFXUtil.FloatToInt(x3);
            int iy3 = GFXUtil.FloatToInt(y3);
            int ix4 = GFXUtil.FloatToInt(x4);
            int iy4 = GFXUtil.FloatToInt(y4);

            //check for pixel sized rectangle
            if (ix1 == ix2  && ix2==ix3 && ix3==ix4 && iy1 == iy2 && iy2==iy3 && iy3==iy4)
            {
                SetPixel(ix1, iy1, color);
                return;
            }

            Line_Internal(ix1, iy1, ix2, iy2, false);
            Line_Internal(ix2, iy2, ix3, iy3, false);
            Line_Internal(ix3, iy3, ix4, iy4, false);
            Line_Internal(ix4, iy4, ix1, iy1, false);
        }

        public override void DrawArc(double x1, double y1, double x2, double y2, double bulge)
        {
            if (thinlinemode != ThinLineModeFlag.OwnThinLines)
            {
                base.DrawArc(x1, y1, x2, y2,bulge);
                return;
            }

            stipplebit = 0x8000;
            
            int clipres = Clipper.ClipArc(x1, y1, x2, y2, bulge, clipbuffer, clipMinX,clipMinY,clipMaxX,clipMaxY);
            for (int idx = 0; idx < clipres; idx += 5)
            {
                int cx = GFXUtil.FloatToInt(clipbuffer[idx]), cy = GFXUtil.FloatToInt(clipbuffer[idx+1]), nx, ny;
                GeomUtil.FlattenArc(clipbuffer[idx], clipbuffer[idx+1], clipbuffer[idx+2], clipbuffer[idx+3], clipbuffer[idx+4], false, flattentol, (x, y, moveto) =>
                {
                    nx = GFXUtil.FloatToInt(x);
                    ny = GFXUtil.FloatToInt(y);
                    Line_Internal(cx, cy, nx, ny, x2 == x && y2 == y); //set last point if last segment
                    cx = nx;
                    cy = ny;
                });
            }
        }

        public override void DrawCircle(double cx, double cy, double radius)
        {
            if (thinlinemode != ThinLineModeFlag.OwnThinLines)
            {
                base.DrawCircle(cx,cy,radius);
                return;
            }


            stipplebit = 0x8000;
            int nx = 0, ny = 0, px = 0, py = 0;

            int buffersize = Clipper.ClipCircle(cx, cy, radius, clipbuffer, clipMinX, clipMinY, clipMaxX, clipMaxY);
            if (buffersize<=0)
                return;

            if (buffersize >= 5) //circle is clipped to arcs
            { //the circle is divided into arc(s)
                
                double bulge, x1, y1, x2, y2;
                for (int l = 0; l < buffersize; l += 5)
                {
                    x1 = clipbuffer[l];
                    y1 = clipbuffer[l + 1];
                    x2 = clipbuffer[l + 2];
                    y2 = clipbuffer[l + 3];
                    bulge = clipbuffer[l + 4];
                    
                    //draw arc here instead of calling DrawArc to avoid arc clipping because we already clipped!
                    GeomUtil.FlattenArc(x1,y1,x2,y2,bulge, true, flattentol, (x, y, moveto) =>
                    {
                        if (moveto)
                        {
                            px = GFXUtil.FloatToInt(x);
                            py = GFXUtil.FloatToInt(y);
                        }
                        else
                        {
                            nx = GFXUtil.FloatToInt(x);
                            ny = GFXUtil.FloatToInt(y);
                            Line_Internal(px, py, nx, ny, false);
                            px = nx;
                            py = ny;
                        }
                    });
                }

                return;
            }
            

            for (double l = -1; l <= 1.0001; l+=2)
            { //2 arcs gives circle  //TODO: make flatten circle in GeomUtil instead
                GeomUtil.FlattenArc(cx + radius*l, cy, cx - radius*l, cy, 1.0, true, flattentol, (x, y, moveto) =>
                {
                    if (moveto)
                    {
                        px = GFXUtil.FloatToInt(x);
                        py = GFXUtil.FloatToInt(y);
                    }
                    else
                    {
                        nx = GFXUtil.FloatToInt(x);
                        ny = GFXUtil.FloatToInt(y);
                        Line_Internal(px, py, nx, ny, false);
                        px = nx;
                        py = ny;
                    }
                });
            }
          
        }


        public override void DrawEllipticArc(double cx, double cy, double aradius, double bradius, double tilt, double startangle, double sweepangle)
        {
            if (thinlinemode != ThinLineModeFlag.OwnThinLines)
            {
                base.DrawEllipticArc(cx, cy, aradius, bradius, tilt, startangle, sweepangle);
                return;
            }

            /* stipplebit = 0x8000;
            
             int px=0,py=0,nx,ny;
             bool first = true;

             GeomUtil.FlattenEllipticArc(cx, cy, aradius, bradius, tilt, startangle, sweepangle, flattentol, true, (x, y,moveto) =>
             {
                 nx = GFXUtil.RealToInt(x);
                 ny = GFXUtil.RealToInt(y);
                 if (!first)
                     Line_Internal(px, py, nx, ny, true); //TODO: can we skip last pixel except on last segment? 
                 else
                     first = false;
                    
                
                 px = nx;
                 py = ny;
             });*/


            int px = 0, py = 0, nx, ny;

            int num = Clipper.ClipEllipticArc(cx, cy, aradius, bradius, tilt, startangle, sweepangle, clipbuffer, clipMinX, clipMinY, clipMaxX, clipMaxY);

            for (int l = 0; l < num; l += 2)
            {

                stipplebit = 0x8000;

                GeomUtil.FlattenEllipticArc(cx, cy, aradius, bradius, tilt, clipbuffer[l], clipbuffer[l + 1] - clipbuffer[l], flattentol, true, (x, y, moveto) =>
                {
                    nx = GFXUtil.FloatToInt(x);
                    ny = GFXUtil.FloatToInt(y);
                    if (!moveto)
                        Line_Internal(px, py, nx, ny, true); //TODO: can we skip last pixel except on last segment? 
                    px = nx;
                    py = ny;
                });

            }
        }


        public override void DrawEllipse(double cx, double cy, double aradius, double bradius, double tilt)
        {
            if (thinlinemode != ThinLineModeFlag.OwnThinLines)
            {
                base.DrawEllipse(cx, cy, aradius, bradius, tilt);
                return;
            }

            /*stipplebit = 0x8000;

            int px = 0, py = 0, nx, ny;
            bool first = true;

            GeomUtil.FlattenEllipse(cx, cy, aradius, bradius, tilt, flattentol, true, (x, y, moveto) =>
            {
                nx = GFXUtil.RealToInt(x);
                ny = GFXUtil.RealToInt(y);
                if (!first)
                    Line_Internal(px, py, nx, ny, true); //TODO: can we skip last pixel except on last segment? 
                else
                    first = false;
                px = nx;
                py = ny;
            });*/


            

            int px = 0, py = 0, nx, ny;

            int num = Clipper.ClipEllipse(cx, cy, aradius, bradius, tilt, clipbuffer, clipMinX, clipMinY, clipMaxX, clipMaxY);

            if (num == 1)
            { //entire arc visible

                stipplebit = 0x8000;
                
                GeomUtil.FlattenEllipse(cx, cy, aradius, bradius, tilt, flattentol, true, (x, y, moveto) =>
                {
                    nx = GFXUtil.FloatToInt(x);
                    ny = GFXUtil.FloatToInt(y);
                    if (!moveto)
                        Line_Internal(px, py, nx, ny, true); //TODO: can we skip last pixel except on last segment? 
                    px = nx;
                    py = ny;
                });
            }
            else
            { //arc is clipped (or possibly invisble when nnum==0)
                for (int l = 0; l < num; l+=2)
                {

                    stipplebit = 0x8000;

                    GeomUtil.FlattenEllipticArc(cx, cy, aradius, bradius, tilt, clipbuffer[l], clipbuffer[l + 1] - clipbuffer[l], flattentol, true, (x, y, moveto) =>
                    {
                        nx = GFXUtil.FloatToInt(x);
                        ny = GFXUtil.FloatToInt(y);
                        if (!moveto)
                            Line_Internal(px, py, nx, ny, true);
                        px = nx;
                        py = ny;
                    });

                }
            }
        }

        public override void DrawPolyLine(bool close, params double[] xy)
        {
            if (thinlinemode != ThinLineModeFlag.OwnThinLines) 
            {
                base.DrawPolyLine(close, xy);
                return;
            }

            stipplebit = 0x8000;

            int n = xy.Length;

            if (xy.Length < 2)
                return;

            int prevx = GFXUtil.FloatToInt(xy[0]);
            int prevy = GFXUtil.FloatToInt(xy[1]);
            int firstx = prevx;
            int firsty = prevy;

            int lastidx = n - 2;

            for (int l = 0; l < n; )
            {
                bool lastpixel = l == lastidx;

                int nextx = GFXUtil.FloatToInt(xy[l++]);
                int nexty = GFXUtil.FloatToInt(xy[l++]);

                Line_Internal(prevx, prevy, nextx, nexty, lastpixel);
                prevx = nextx;
                prevy = nexty;
            }

            if (close)
                Line_Internal(prevx, prevy, firstx, firsty, false);
        }

       /* public override void DrawPath(GFXPath path)
        {
            if (thinlinemode != ThinLineModeFlag.OwnThinLines)
            {
                base.DrawPath(path);
                return;
            }

            stipplebit = 0x8000;
            int cx=0,cy=0;
            path.Flatten(flattentol, (x, y, moveto) =>
            {
                if (moveto)
                {
                    cx = GFXUtil.RealToInt(x);
                    cy = GFXUtil.RealToInt(y);
                }
                else
                {
                    int nx = GFXUtil.RealToInt(x);
                    int ny = GFXUtil.RealToInt(y);
                    Line_Internal(cx,cy,nx,ny, true);
                    cx = nx;
                    cy = ny;
                }
            });
        }*/


        public override void DrawPath(GFXPath path)
        {
            if (thinlinemode != ThinLineModeFlag.OwnThinLines)
            {
                base.DrawPath(path);
                return;
            }

            stipplebit = 0x8000;

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

        private void InternalDrawBezier(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            int n = Clipper.ClipBezier(x1, y1, x2, y2, x3, y3, x4, y4, clipbuffer, clipMinX, clipMinY, clipMaxX, clipMaxY);
            int penx=0,peny=0;

            for (int l = 0; l < n; l += 2)
            {
                GeomUtil.FlattenBezier(x1, y1, x2, y2, x3, y3, x4, y4, true, flattentol, (x, y, moveto) =>
                {
                    int nextx=GFXUtil.FloatToInt(x);
                    int nexty=GFXUtil.FloatToInt(y);

                    if (!moveto)
                        Line_Internal(penx, peny, nextx, nexty,false);

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

            /*int n = Clipper.ClipBezier(x1, y1, x2, y2, x3, y3, x4, y4, clipbuffer, clipMinX, clipMinY, clipMaxX, clipMaxY);
            int penx = 0, peny = 0;

            for (int l = 0; l < n; l += 2)
            {
                GeomUtil.FlattenBezier(x1, y1, x2, y2, x3, y3, x4, y4, true, flattentol, (x, y, moveto) =>
                {
                    int nextx = GFXUtil.RealToInt(x);
                    int nexty = GFXUtil.RealToInt(y);

                    if (!moveto)
                        Line_Internal(penx, peny, nextx, nexty, false);

                    penx = nextx;
                    peny = nexty;

                }, clipbuffer[l], clipbuffer[l + 1]);
            }*/
        }

        /*public override void DrawPathT(GFXPath path)
        {
            if (thinlinemode != ThinLineModeFlag.OwnThinLines)
            {
                base.DrawPathT(path);
                return;
            }

            stipplebit = 0x8000;
            int cx = 0, cy = 0;

            path.TransformFlatten(flattentol, Transform, (x, y, moveto) =>
            {
                if (moveto)
                {
                    cx = GFXUtil.RealToInt(x);
                    cy = GFXUtil.RealToInt(y);
                }
                else
                {
                    int nx = GFXUtil.RealToInt(x);
                    int ny = GFXUtil.RealToInt(y);
                    Line_Internal(cx, cy, nx, ny, true);
                    cx = nx;
                    cy = ny;
                }
            });
        }*/

        

        public override void DrawPathT(GFXPath path)
        {
            if (thinlinemode != ThinLineModeFlag.OwnThinLines)
            {
                base.DrawPathT(path);
                return;
            }

            stipplebit = 0x8000;
            
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


        //TODO: check that all necesary functions are overridden. For example CreatePicture... functions needs overrides
    }
}
