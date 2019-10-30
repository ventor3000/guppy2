using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Guppy2.GFX.Windows;
using Guppy2.Calc.Geom2d;
using Guppy2.Calc;

namespace Guppy2.GFX.Windows
{

    public class PainterGDIPlus : Painter
    {
        protected double[] clipbuffer = new double[50]; //used for output from clipping algoritms
        
        private static Bitmap single_pixel_bitmap = new Bitmap(1, 1); //used for set pixel (faster than pinvoke of SetPixel and CLR compilant)
        private static int single_bitmap_rgb = -1; //used to check if we need to alter our single pixel bitmap before drawing it in SetPixel
        
        protected Graphics gfx;
        private IntPtr hdc=IntPtr.Zero; //hdc of target we paint to, must be released on dispose if not IntPtr.Zero

        private Pen linepen = null;
        private Brush linebrush = null; //used for text
        private Brush fillbrush = null; //used for filled shapes
        private Color fgcolor = System.Drawing.Color.Black;
        private Color bgcolor = System.Drawing.Color.Empty;
        

        private GraphicsState oldgfxstate = null; //if this is null, we owns graphic and releases it when done, otherwise it's restored to this


        private PictureGDIPlus target_picture = null;  //if not null, we are drawing to a GDI image

        //precomputed in constructor=>SetupDefaults
        private int width;
        private int height;
        private double pixels_per_mm_x;
        private double pixels_per_mm_y;


        //clipping state
        protected double clipMinX, clipMinY, clipMaxX, clipMaxY; 
        
        

        //Font state
        /*Font _gdiFont = null;
        double _fontAscentPixels;   //ascent height in pixels of the currently selected font
        double _fontLineHeight;     //line spacing in pixels of the currently selected font*/
        
        // pattern state
        private PictureGDIPlus _gdiPattern = null; //pattern we use or null if not set
        private bool _ownsGDIPattern = false;   //if we owns the current pattern and should dispose it on set/reset/dispose
        
        
        //font handling
        private class CachedFont
        {
            internal readonly Font Font;
            internal readonly double AscentPixels;
            internal readonly double DescentPixels;
            internal readonly double LineSpacingPixels;

            public CachedFont(Font f, double linespace_pixels, double ascent_pixels, double descent_pixels)
            {
                this.Font = f;
                this.AscentPixels = ascent_pixels;
                this.DescentPixels = descent_pixels;
                this.LineSpacingPixels = linespace_pixels;
            }
        }
        static Dictionary<string, CachedFont> _fontCache = new Dictionary<string, CachedFont>();    // all GDI fonts ever created.
        


        public PainterGDIPlus(Control ctrl)
        {
            gfx = ctrl.CreateGraphics();
            oldgfxstate = null; //signals we're owning the graphic
            SetupDefaults();
        }

        
        public PainterGDIPlus(Graphics graphics)
        {
            Init(graphics);
        }

        public PainterGDIPlus(Picture srvimg)
        {
            TextureBrush tb = MapPattern(srvimg);

            target_picture = srvimg as PictureGDIPlus;
            if (target_picture == null)
                throw new Exception("GDI Painter can only draw to server image of its own format");

            target_picture.UncacheTexture(); //texture is now invalid (recreated on demand)
            gfx = Graphics.FromImage(target_picture.Image);

            oldgfxstate = null; //signals we're owning the graphic
            SetupDefaults();
        }

        public PainterGDIPlus(PictureWinRGB dib)
        {
            hdc = GDI.CreateCompatibleDC(IntPtr.Zero);
            GDI.SelectObject(hdc, dib.Handle);

            gfx = Graphics.FromHdc(hdc);
            oldgfxstate = null; //we owns the Graphic object and releases it when done
            SetupDefaults();
        }

        /// <summary>
        /// Creates a painter, that is is uninitialized unless 'forscreen' is true, in which case it 
        /// is a painter compatible with the currrent screen.
        /// </summary>
        public PainterGDIPlus(bool forscreen)
        {
            if (forscreen)
            {
                hdc = GDI.CreateCompatibleDC(hdc);
                gfx = Graphics.FromHdc(hdc);
                oldgfxstate = null;
                SetupDefaults();
            }
        }


        protected void Init(Graphics graphics)
        {
            gfx = graphics;
            oldgfxstate = gfx.Save(); //signals we dont own gfx, and it should be restored when done
            SetupDefaults();
        }

        private void SetupDefaults()
        {
            gfx.ResetTransform();

            pixels_per_mm_x = gfx.DpiX / 25.4;
            pixels_per_mm_y = gfx.DpiY / 25.4;

            if (!GetClientSizeFromGraphics(gfx, out width, out height))
                throw new Exception("Failed to get width and height for Graphics");

            //we use upwards coordinate system
            gfx.TranslateTransform(0f, (float)(height - 1));
            gfx.ScaleTransform(1f, -1f);
            
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;  //default ugly image scaling (and no antialiasig which is Graphics default)
            gfx.SmoothingMode = SmoothingMode.None;

            //base.Clip = new Rectangle2i(0, 0, Width - 1, Height - 1);
            
            //initial state of clipping
            Clip = null;    //to setup resetted clipping variables
        }



        private bool GetClientSizeFromGraphics(Graphics g, out int w, out int h)
        {
            IntPtr hdc = g.GetHdc();
            try
            {
                w = h = -1;

                if (hdc != IntPtr.Zero)
                {
                    ///////////////////////////////
                    //Try for Graphic from control:
                    ///////////////////////////////

                    //try for hdc from window
                    var hwnd = GDI.WindowFromDC(hdc);
                    if (hwnd != IntPtr.Zero)
                    {
                        RECT rc;
                        GDI.GetClientRect(hwnd, out rc);
                        w = rc.Right - rc.Left;
                        h = rc.Bottom - rc.Top;

                        return true;
                    }

                    //////////////////////////////
                    //Try for graphic from bitmap:
                    //////////////////////////////
                    IntPtr hbitmap;
                    hbitmap = GDI.GetCurrentObject(hdc, GDI.ObjectType.OBJ_BITMAP);
                    if (hbitmap != IntPtr.Zero)
                    {
                        GDI.BITMAP bi = new GDI.BITMAP();
                        int siz = GDI.GetObject(hbitmap, 0, IntPtr.Zero);
                        siz = GDI.GetBitmapObject(hbitmap, siz, ref bi);

                        if (siz != 0)
                        {
                            w = bi.bmWidth;
                            h = bi.bmHeight;
                            return true;
                        }

                    }
                }

                return false;
            }
            finally
            {
                if (hdc != IntPtr.Zero)
                    gfx.ReleaseHdc(hdc);
            }


        }



        #region ATTRIBUTES


        internal static Color RGBToColor(int value)
        {
            return System.Drawing.Color.FromArgb(255 - RGB.GetAlpha(value), RGB.GetRed(value), RGB.GetGreen(value), RGB.GetBlue(value));
        }

        public override int Color
        {
            get
            {
                return base.Color;
            }
            set
            {
                fgcolor = RGBToColor(value);
                ReleaseObjects(true, FillStyle != FillStyle.Pattern); //only release brush if not patterned (in which case color is not used)
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
                if (value != Opacity) //opacity affects all drawing
                    ReleaseObjects(true, true);
                base.Opacity = value;
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
                if (value != base.AntiGrain)
                {
                    if (value)
                    {
                        gfx.SmoothingMode = SmoothingMode.AntiAlias;
                        gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

                        //HINT: bug: antialias gridfit makes hanging paramters that destroys for later painters (notably GetPath in GFXPath)
                        //gfx.TextRenderingHint = TextRenderingHint.AntiAliasGridFit; 
                    }
                    else
                    {
                        gfx.SmoothingMode = SmoothingMode.None;
                        gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
                        
                        //gfx.TextRenderingHint = TextRenderingHint.SystemDefault;
                    }
                    
                    
                    
                    base.AntiGrain = value;
                }
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
                if (FillStyle != value)
                {
                    ReleaseObjects(false, true);
                }
                base.FillStyle = value;
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
                if (Hatch != value)
                {
                    FillStyle = FillStyle.Hatch; //change to hatch mode when setting the hatch
                    ReleaseObjects(false, true);
                }
                base.Hatch = value;
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
                {
                    ReleaseObjects(true, false);
                }
                base.LineWidth = value;
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
                {
                    ReleaseObjects(true, false);
                }

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
                if (LineStyleDashes != value)
                {
                    ReleaseObjects(true, false);
                }
                base.LineStyleDashes = value;
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
                if (base.LineJoin != value)
                    ReleaseObjects(true, false);
                base.LineJoin = value;
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
                if (base.EndCaps != value)
                    ReleaseObjects(true, false);
                base.EndCaps = value;
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
                if (Pattern != value)
                {
                    ReleaseObjects(false, true);

                    if (_ownsGDIPattern && _gdiPattern != null) //release old pattern if we owns it
                    {
                        _gdiPattern.Dispose();
                        _gdiPattern = null;
                        _ownsGDIPattern = false;
                    }


                    _gdiPattern = MakeCompatiblePicture(value) as PictureGDIPlus;
                    _ownsGDIPattern = _gdiPattern != value;
                }
                base.Pattern = value;
            }
        }

        public override Rectangle2i Clip
        {
            get
            {
                return base.Clip;
            }
            set
            {

                if (value == null)
                {
                    base.Clip = null; //to reset Clip rectangle
                    gfx.ResetClip();
                }
                else
                {
                    int xmin = value.XMin;
                    int ymin = value.YMin;
                    int xmax = value.XMax;
                    int ymax = value.YMax;

                    MathUtil.Sort(ref xmin, ref xmax);
                    MathUtil.Sort(ref ymin, ref ymax);

                    double w = xmax - xmin;
                    double h = ymax - ymin;

                    gfx.SetClip(new RectangleF((float)(xmin - 0.5), (float)(ymin - 0.5), (float)(w + 1), (float)(h + 1)));
                    base.Clip = value;
                }

                //store non-cast quick access to clipping params
                clipMinX = Clip.XMin;
                clipMinY = Clip.XMin;
                clipMaxX = Clip.XMax;
                clipMaxY = Clip.YMax;
            }
        }

        public override string Info
        {
            get { return "GDI+"; }
        }


        #endregion

        #region LINE_PRIMITIVES

        public override void SetPixel(int x, int y, int rgb)
        {
            if (x < clipMinX || y < clipMinY || x > clipMaxX || y > clipMaxY)
                return;

            if (single_bitmap_rgb != rgb)
            {
                single_pixel_bitmap.SetPixel(0, 0, RGBToColor(rgb));
                single_bitmap_rgb = rgb;
            }

            gfx.DrawImageUnscaled(single_pixel_bitmap, x,y);
           // gfx.DrawImageUnscaled(single_pixel_bitmap, 0, height - 2);
            
            
            //IntPtr hdc = gfx.GetHdc();
            //int bgr = GFXUtil.RGBToBGR(rgb);
            //GDI.SetPixel(hdc, x, height - 1 - y, bgr);
            //GDI.SetPixel(hdc, 0,height-2, bgr);

            //gfx.ReleaseHdc(hdc);
        }

        public override int GetPixel(int x, int y)
        {
            //GDI+ has no way of setting a pixel, fallback to old GDI
            IntPtr hdc = gfx.GetHdc();
            int bgr;

            bgr = GDI.GetPixel(hdc, x, y - height - 1);
            gfx.ReleaseHdc(hdc);

            return RGB.FromBGR(bgr);
        }

        public override void DrawRectangle(double x1, double y1, double x2, double y2)
        {
            var oldt = Transform;
            Transform = null;
            DrawRectangleT(x1, y1, x2, y2);
            Transform = oldt;
        }


        public override void DrawPolyLine(bool close, params double[] xy)
        {
            var oldt = Transform;
            Transform = null;
            DrawPolyLineT(close, xy);
            Transform = oldt;
        }

        public override void DrawPolyLineT(bool close, params double[] xy)
        {
            GFXPath pth = new GFXPath();
            

            int xyidx = 0;
            int numpt = xy.Length >> 1;

            for (int l = 0; l < numpt; l++)
            {
                double x = xy[xyidx++];
                double y = xy[xyidx++];

                if (l == 0)
                    pth.MoveTo(x, y);
                else
                    pth.LineTo(x, y);
            }
            if (close)
                pth.CloseSubPath();

            DrawPathT(pth);
        }

      
       

        private void SmallPrimitive(double x, double y)
        {
            UpdateLinePen();
            gfx.DrawLine(linepen, (float)x, (float)y, (float)(x + 0.01), (float)(y));
        }

        public override void DrawLine(double x1, double y1, double x2, double y2)
        {
            if (x1 == x2 && y1 == y2)
            {
                SmallPrimitive(x1, y1);
                return;
            }

            if (Clipper.ClipLine(ref x1, ref y1, ref x2, ref y2, clipMinX, clipMinY, clipMaxX, clipMaxY)) //help GDI which sucks at large coordinates
            {
                UpdateLinePen();
                gfx.DrawLine(linepen, (float)x1, (float)y1, (float)x2, (float)y2);
            }
        }

        public override void DrawLineT(double x1, double y1, double x2, double y2)
        {
            Transform.Apply(x1, y1, out x1, out y1, true);
            Transform.Apply(x2, y2, out x2, out y2, true);
            DrawLine(x1, y1, x2, y2);
        }

        public override void DrawCircle(double cx, double cy, double radius)
        {
            InternalEllipse(cx, cy, radius, radius, 0.0, 0.0, MathUtil.Deg360, false);
        }

        public override void DrawEllipse(double cx, double cy, double aradius, double bradius, double angle)
        {
            InternalEllipse(cx, cy, aradius, bradius, angle, 0.0, MathUtil.Deg360, false);
        }

       
        public override void DrawEllipticArc(double cx, double cy, double aradius, double bradius, double tilt, double startangle, double sweepangle)
        {
            InternalEllipse(cx, cy, aradius, bradius, tilt, startangle, sweepangle, false);
        }

        private bool SmallPathGDIBugWorkaround(GFXPath path,Transform2d tr)
        {
            //work around bug with 'out of memory' exception for very small paths
            //and non-continuous line styles
            if ((LineWidth>=1.5 || LineStyle != LineStyle.Continuous) && path.IsTransformedSmallerThan(tr, 1.5,1.5))
            {
                if (path.PathPoints.Count > 0)
                    SmallPrimitive(path.PathPoints[0].X, path.PathPoints[0].Y); //draw something to indicate a very small primitive here

                return true; //bug fixed
            }

            return false;  //no bug fix needed
        }

        public override void DrawPath(GFXPath path)
        {
            if (SmallPathGDIBugWorkaround(path, Transform2d.Identity)) return;

            UpdateLinePen();
            gfx.DrawPath(linepen, GetGraphicsPath(path));
        }

        public override void DrawPathT(GFXPath path)
        {
            if (SmallPathGDIBugWorkaround(path, Transform)) return;

            GraphicsPath gp = GetGraphicsPath(path);

            if (!Transform.IsIdentity)
            {
                gp = (GraphicsPath)gp.Clone();
                gp.Transform(TransformToMatrix(Transform));
            }

            UpdateLinePen();
            gfx.DrawPath(linepen, gp);
        }

        #endregion

        #region IMAGES

        public override void DrawPicture(Picture p, double x, double y)
        {

            if (p is PictureWinRGB)
            {

                int ix1 = GFXUtil.FloatToInt(x);
                int iy1 = GFXUtil.FloatToInt(y);
                int ix2 = ix1 + p.Width - 1;
                int iy2 = iy1 + p.Height - 1;
                //handle this case separatly for greater speed
               // DrawPixels(p.Width,p.Height,(p as PictureWinRGB).Pixels,null, ix1,iy1,ix2,iy2,0,0);
                DrawPixels(p.Width, p.Height, (p as PictureWinRGB).Pixels, null, ix1, iy1, p.Width, p.Height, 0, 0, p.Width, p.Height);
            }
            else
            {
                Image image=MapImage(p);
                if (image == null)
                {
                    base.DrawIncompatiblePicture(p, x, y);
                    return;
                }

                if (Opacity < 0.999)
                {
                    ColorMatrix cm = new ColorMatrix();
                    cm.Matrix33 = (float)Opacity;
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetColorMatrix(cm);
                    gfx.DrawImage(image,
                        new Rectangle(GFXUtil.FloatToInt(x), GFXUtil.FloatToInt(y + p.Height), GFXUtil.FloatToInt(p.Width), GFXUtil.FloatToInt(-p.Height)),
                        0f,0f,(float)p.Width,(float)p.Height,GraphicsUnit.Pixel,attr);

                }
                else
                {
                    gfx.DrawImage(image, (float)x, (float)(y + p.Height-1), (float)p.Width, (float)-p.Height);
                }
            }
        }
        /*
        public override void Blit(Picture p, int dest_x1, int dest_y1, int dest_x2, int dest_y2, int src_x, int src_y)
        {
            if (p is PictureWinRGB)
            {
                //handle this case separatly for greater speed (used for IGEMS backbuffer)
                //DrawDIB32(p as PictureWinRGB, dest_x1,dest_y1,dest_x2,dest_y2,src_x,src_y);
                DrawPixels(p.Width,p.Height,(p as PictureWinRGB).Pixels,null, dest_x1, dest_y1, dest_x2, dest_y2, src_x, src_y);

//                DrawPixels(p.Width, p.Height, (p as PictureWinRGB).Pixels, null, dest_x1, dest_y1, (dest_x2 - dest_x1) + 1, (dest_y2 - dest_y1) + 1, src_x, src_y, (dest_x2 - dest_x1) + 1, (dest_y2 - dest_y1) + 1);
            }
            else if (p is PictureGDIPlus)
            {
                PictureGDIPlus pgdi = p as PictureGDIPlus;
                int w = dest_x2 - dest_x1 + 1;
                int h = dest_y2 - dest_y1 + 1;
                

                gfx.DrawImage(pgdi.Image,
                    //new Rectangle(dest_x1,dest_y1+h-1,w,-h),
                    new Rectangle(dest_x1, dest_y1 + h - 1, w, -h),
                    new Rectangle(src_x, p.Height - h - src_y, w, h), GraphicsUnit.Pixel);
            }
            else 
            {
                //Incompatible image rendering, convert to soft image (maybe already is soft image)
                SoftPicture sp = p.ToSoftPicture();

                DrawPixels(sp.Width, sp.Height, IntPtr.Zero, sp.Pixels, dest_x1,dest_y1,dest_x2,dest_y2,src_x,src_y);

                //if p was not already a softpicture, sp is a newly allocated image, release it
                if (sp != p)
                    sp.Dispose();
            }
        }*/


        public override void Blit(Picture p, int dst_x, int dst_y, int dst_w, int dst_h, int src_x, int src_y, int src_w, int src_h)
        {
            if (p is PictureWinRGB)
            {
                //handle this case separatly for greater speed (used for IGEMS backbuffer)
                //DrawDIB32(p as PictureWinRGB, dest_x1,dest_y1,dest_x2,dest_y2,src_x,src_y);
                DrawPixels(p.Width, p.Height, (p as PictureWinRGB).Pixels, null, dst_x, dst_y, dst_w, dst_h, src_x, src_y, src_w, src_h);
            }
            else if (p is PictureGDIPlus)
            {
                PictureGDIPlus pgdi = p as PictureGDIPlus;
                //int w = dest_x2 - dest_x1 + 1;
                //int h = dest_y2 - dest_y1 + 1;
                /*gfx.DrawImage(pgdi.Image,
                    //new Rectangle(dest_x1,dest_y1+h-1,w,-h),
                    new Rectangle(dest_x1, dest_y1 + h-1 , w, -h),
                    new Rectangle(src_x,p.Height-h-src_y,w,h),GraphicsUnit.Pixel);*/

                //TODO: try this case

                gfx.DrawImage(pgdi.Image,
                    //new Rectangle(dest_x1,dest_y1+h-1,w,-h),
                    new Rectangle(dst_x, dst_y + dst_h - 1, dst_w, -dst_h),
                    new Rectangle(src_x, p.Height - src_h - src_y, src_w, src_h), GraphicsUnit.Pixel);
            }
            else
            {
                //Incompatible image rendering, convert to soft image (maybe already is soft image)
                SoftPicture sp = p.ToSoftPicture();

                DrawPixels(sp.Width, sp.Height, IntPtr.Zero, sp.Pixels, dst_x, dst_y, dst_w, dst_h, src_x, src_y,src_w,src_h);

                //if p was not already a softpicture, sp is a newly allocated image, release it
                if (sp != p)
                    sp.Dispose();
            }
        }
        


        /// <summary>
        /// Draw pixels referenced by:
        /// 1) pix1 if not IntPtr.Zero
        /// 2) otherwise pix2 if not null
        /// </summary>
        private void DrawPixels(int picwidth, int picheight, IntPtr pix1, int[] pix2, int dst_x, int dst_y, int dst_width, int dst_height, int src_x, int src_y,int src_width,int src_height)
        {

            dst_y = Height - 1 - dst_y - dst_height;    //alter y for bottom up picture

            //TODO: try for diffrent positions and scale in src<->dst, only tried for 1:1 mapping
            
            BITMAPINFO bi = new BITMAPINFO();
            bi.biSize = (UInt32)Marshal.SizeOf(bi);
            bi.biBitCount = 32;
            bi.biClrUsed = 0;
            bi.biClrImportant = 0;
            bi.biCompression = 0; // BI_RGB
            bi.biHeight = picheight;
            bi.biWidth = picwidth;
            bi.biPlanes = 1;
            bi.biSizeImage = 0;

            IntPtr ctx = gfx.GetHdc();
            if (ctx != IntPtr.Zero)
            {
                //var xf=new GDI.XFORM(1f,0f,0f,-1f,0f,(float)(Height-1));
                //GDI.SetWorldTransform(ctx,ref xf);
                int suc;


                if (pix1 != IntPtr.Zero) //pixels are given by a IntPtr array
                    suc = GDI.StretchDIBits(ctx, dst_x, dst_y, dst_width, dst_height, src_x, src_y, src_width, src_height, pix1, ref bi, 0, TernaryRasterOperations.SRCCOPY); //0=DIB_RGB_COLORS
                else if (pix2 != null) //pixels are given by an int array
                    suc = GDI.StretchDIBits(ctx, dst_x, dst_y, dst_width, dst_height, src_x, src_y, src_width, src_height, pix2, ref bi, 0, TernaryRasterOperations.SRCCOPY); //0=DIB_RGB_COLORS

                gfx.ReleaseHdc(ctx);
            }
        }
        
        public override void DrawPictureT(Picture p, double x, double y)
        {
            Image image = MapImage(p);
            if (image == null)
            {
                base.DrawIncompatiblePictureT(p, x, y);
                return;
            }
            var t = Transform;

            if (image != null)
            {
                double w = image.Width, h = image.Height;

                Point2d ll, lr, tl;
                ll = new Point2d(x, y + h);
                lr = new Point2d(x + w, y + h);
                tl = new Point2d(x, y);

                ll = ll.GetTransformed(t);
                lr = lr.GetTransformed(t);
                tl = tl.GetTransformed(t);

                if (Opacity < 0.999)
                { //alphablend!
                    ColorMatrix cm = new ColorMatrix();
                    cm.Matrix33 = (float)Opacity;
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetColorMatrix(cm);
                    gfx.DrawImage(image,
                        new PointF[] { new PointF((float)ll.X, (float)ll.Y), new PointF((float)lr.X, (float)lr.Y), new PointF((float)tl.X, (float)tl.Y) },
                        new RectangleF(0f, 0f, (float)w, (float)h), GraphicsUnit.Pixel,attr);
                }
                else
                {
                    gfx.DrawImage(image,
                        new PointF[] { new PointF((float)ll.X, (float)ll.Y), new PointF((float)lr.X, (float)lr.Y), new PointF((float)tl.X, (float)tl.Y) },
                        new RectangleF(0f, 0f, (float)w, (float)h), GraphicsUnit.Pixel);
                }
            }
        }



        private TextureBrush MapPattern(Picture si)
        {
            if (_gdiPattern == null)
                return null;
            return _gdiPattern.GetTextureBrush(Opacity);

            /*PictureGDIPlus gdip = si as PictureGDIPlus;
            if (gdip != null)
                return gdip.Texture;*/
            /* else
             {
                 gdip = MakeCompatiblePicture(si) as PictureGDIPlus;
                 if (gdip != null)
                     return gdip.Texture;
             }*/


            //   return null;
        }

        private Image MapImage(Picture si)
        {
            PictureGDIPlus gdip = si as PictureGDIPlus;
            if (gdip != null)
                return gdip.Image;

            return null;
        }

        public override Picture CreatePictureFromSize(int width, int height)
        {
            return new PictureGDIPlus(width, height);
        }

        public override Picture CreatePictureFromStream(Stream str)
        {
            return new PictureGDIPlus(Bitmap.FromStream(str));
        }

        public override Picture CreatePictureFromSoftPicture(SoftPicture img)
        {
            return new PictureGDIPlus(img);
        }

        public override Picture MakeCompatiblePicture(Picture p)
        {
            if (p == null)
                return null;
            if (p is PictureGDIPlus)
                return p;

            SoftPicture sp = p.ToSoftPicture();
            return CreatePictureFromSoftPicture(sp);
        }

        public override void CopyPicture(ref Picture pic, int x1, int y1, int x2, int y2)
        {


            //flip y becuse we're using BitBlt which knows nothing of flipped coordinate systems
            y1 = Height - 1 - y1;
            y2 = Height - 1 - y2;



            MathUtil.Sort(ref x1, ref x2);
            MathUtil.Sort(ref y1, ref y2);

            int wi = x2 - x1+1;
            int he = y2 - y1+1;
            
            PictureGDIPlus picgdi = pic as PictureGDIPlus;

            if (picgdi == null)
            { //recreate image

                if (pic != null) //picgdi is null but not pic => invalid image format, dispose old one
                    pic.Dispose();

                pic = picgdi = CreatePictureFromSize(wi, he) as PictureGDIPlus;
            }
            else if (picgdi.Width < wi || picgdi.Height < he)
            {
                //need to recreate picture. Grow only
                var oldpic = pic;
                pic = picgdi = CreatePictureFromSize(Math.Max(wi, pic.Width), Math.Max(he, pic.Height)) as PictureGDIPlus;
                oldpic.Dispose();
            }


            picgdi.UncacheTexture();    //forget/ recreate on demand, the texture for this image


            if (target_picture != null) //source is an image
            {
                Bitmap bmp = target_picture.Image as Bitmap;
                if (bmp != null)
                {

                    using (Graphics g = Graphics.FromImage(picgdi.Image))
                    {
                        IntPtr srchdc = gfx.GetHdc();
                        IntPtr desthdc = g.GetHdc();

                        IntPtr hbitmap = bmp.GetHbitmap();
                        IntPtr oldbitmap=GDI.SelectObject(srchdc, hbitmap);

                        GDI.BitBlt(desthdc, 0, picgdi.Height - 1 - he, wi, he, srchdc, x1, y1, TernaryRasterOperations.SRCCOPY);

                        GDI.SelectObject(srchdc, oldbitmap);
                        GDI.DeleteObject(hbitmap);
                        gfx.ReleaseHdc(srchdc);
                        g.ReleaseHdc(desthdc);
                    }
                }

                //bitblt wont work if target is a gdi+ image
                /*using (Graphics g = Graphics.FromImage(picgdi.Image))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.DrawImage(target_picture.Image, 0, 0, wi, he);
                } */
            }
            else //source is not an image
            {
                using (Graphics g = Graphics.FromImage(picgdi.Image))
                {
                    IntPtr srchdc = gfx.GetHdc();
                    IntPtr desthdc = g.GetHdc();


                    //bool suc = GDI.BitBlt(desthdc, 0, 0, wi, he, srchdc, x1, y1, TernaryRasterOperations.SRCCOPY);
                    bool suc = GDI.BitBlt(desthdc, 0, picgdi.Image.Height - he , wi, he, srchdc, x1, y1, TernaryRasterOperations.SRCCOPY);
                    gfx.ReleaseHdc(srchdc);
                    g.ReleaseHdc(desthdc);

                }
            }
        }

        #endregion

        private void InternalEllipse(double cx, double cy, double aradius, double bradius, double tilt,
                                     double startangle, double sweepangle, bool fill)
        {
            //Because GDI sucks on drawing ellipses using extreme eccentricity or large coordinates, and is generally unstable,
            //we do it ourselves by converting to bezier paths and clipping them.

            bool fullellipse = sweepangle >= MathUtil.Deg360;

            int num;
            if (fullellipse)
                num = Clipper.ClipEllipse(cx, cy, aradius, bradius, tilt, clipbuffer, clipMinX, clipMinY, clipMaxX,clipMaxY);
            else
                num = Clipper.ClipEllipticArc(cx, cy, aradius, bradius, tilt, startangle, sweepangle, clipbuffer,
                                              clipMinX, clipMinY, clipMaxX, clipMaxY);

            if (num == 1) num = 2; //full ellipse, we still use 2 parameters of clipbuffer

            for (int l = 0; l < num; l += 2)
            {

                GFXPath pth = new GFXPath();
                List<double> xy = new List<double>();

                startangle = clipbuffer[l];
                sweepangle = clipbuffer[l + 1] - clipbuffer[l];

                Point2d p = GeomUtil.EvalEllipseParam(cx, cy, aradius, bradius, tilt,
                                                     GeomUtil.EllipseAngleToParam(startangle, aradius, bradius));
                GeomUtil.EllipticArcToBeziers(cx, cy, aradius, bradius, tilt, startangle, sweepangle, xy);

                pth.MoveTo(p.X, p.Y);

                if (xy.Count == 2)
                {
                    pth.LineTo(xy[0], xy[1]);
                }
                else
                {
                    for (int i = 0; i < xy.Count; i += 6)
                        pth.BezierTo(xy[i], xy[i + 1], xy[i + 2], xy[i + 3], xy[i + 4], xy[i + 5]);
                }

                if (fill)
                    FillPath(pth);
                else
                    DrawPath(pth);
            }


        }

        #region FILLED_PRIMITIVES

        public override void FillRectangle(double x1, double y1, double x2, double y2)
        {
            UpdateBrush(null);

            MathUtil.Sort(ref x1, ref x2);
            MathUtil.Sort(ref y1, ref y2);
            float w = (float)(x2 - x1);
            float h = (float)(y2 - y1);
            gfx.FillRectangle(fillbrush, (float)(x1 - 0.5), (float)(y1 - 0.5), w + 1.0f, h + 1.0f);
        }

        public override void FillGradientRectangle(double x1, double y1, double x2, double y2, int y1color, int y2color)
        {
            if (MathUtil.Sort(ref y1, ref y2))
            { //if switching y coords we have to swap colors to
                int tc = y1color;
                y1color = y2color;
                y2color = tc;
            }
            MathUtil.Sort(ref x1, ref x2);


            using (LinearGradientBrush lb = new LinearGradientBrush(new PointF(0f, (float)y1-1), new PointF(0f, (float)y2 + 1.5f), RGBToColor(y1color), RGBToColor(y2color)))
            {
                //gfx.FillRectangle(lb, (float)x1, (float)y1-0.5f, (float)(x2 - x1 +1 ), (float)(y2 - y1 )+0.5f);
                gfx.FillRectangle(lb, GFXUtil.FloatToInt(x1), GFXUtil.FloatToInt(y1-1), GFXUtil.FloatToInt(x2 - x1 + 1), GFXUtil.FloatToInt(y2 - y1+1));

                
            }
        }

        public override void FillPath(GFXPath path)
        {
            UpdateBrush(null);
            GraphicsPath gp = GetGraphicsPath(path);
            gp.FillMode = this.FillMode == FillMode.EvenOdd ? System.Drawing.Drawing2D.FillMode.Alternate : System.Drawing.Drawing2D.FillMode.Winding;
            gfx.FillPath(fillbrush, gp);
        }

        public override void FillPathT(GFXPath path)
        {
            GraphicsPath gp = GetGraphicsPath(path);

            if (!Transform.IsIdentity)
            {
                gp = (GraphicsPath)gp.Clone();
                gp.Transform(TransformToMatrix(Transform));
            }

            UpdateBrush(Transform);
            gp.FillMode = this.FillMode == FillMode.EvenOdd ? System.Drawing.Drawing2D.FillMode.Alternate : System.Drawing.Drawing2D.FillMode.Winding;
            gfx.FillPath(fillbrush, gp);
        }

        #endregion

        #region TEXT


      
        internal static System.Drawing.Drawing2D.Matrix TransformToMatrix(Transform2d tm)
        {
            return new System.Drawing.Drawing2D.Matrix((float)tm.AX, (float)tm.AY, (float)tm.BX, (float)tm.BY, (float)tm.TX, (float)tm.TY);
        }

       /* public override void DrawTextT(Typeface typeface, string txt, double x, double y, double size, double angle, TextAlign align)
        {

            var t = Transform;

            Font font = MapTypeface(typeface);
            if (font == null) return; //no font set

            if (size < MathUtil.Epsilon)  //MultiplyTransform cant manage zero length axes of matrices
            {
                SmallPrimitive(x, y);
                return;
            }

            UpdateLineBrush();

            Matrix oldtrans = gfx.Transform;
            var tm = typeface.GetTextTransform(this, txt, x, y, size, angle, align, t);
            gfx.MultiplyTransform(TransformToMatrix(tm));
            
            gfx.DrawString(txt, font, linebrush, 0f, 0f, StringFormat.GenericTypographic);
            gfx.Transform = oldtrans;
        }*/


        private FontStyle FindAFontStyle(FontFamily family)
        {
            //gets a style available for a font, prioritizing regular.
            //this is because creating fonts with a style that is not available
            //throws an exception on vista and win7 (but not on win8)
            if (family.IsStyleAvailable(FontStyle.Regular)) return FontStyle.Regular;
            if (family.IsStyleAvailable(FontStyle.Bold)) return FontStyle.Bold;
            if (family.IsStyleAvailable(FontStyle.Italic)) return FontStyle.Italic;
            if (family.IsStyleAvailable(FontStyle.Underline)) return FontStyle.Underline;
            if (family.IsStyleAvailable(FontStyle.Strikeout)) return FontStyle.Strikeout;

            return FontStyle.Regular;   //dont know what to do(??) no styles available
        }


        /// <summary>
        /// Looks in the cache for a created font. If not found, it tries to create it and precompute data for it,
        /// and then caches it. If this fails, null is returned, which means the fon is non existing.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private CachedFont FindOrCreateFont(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name) || name[0] == '*') // name forces builtin font...
                    return null;    // ...so we return not found in GDI 

                CachedFont res;

                if (name == null) return null;

                if (_fontCache.TryGetValue(name, out res))
                    return res;

                Font font;

                //font not pre-created, create it now

                FontFamily family = new FontFamily(name);
                FontStyle fontstyle = FindAFontStyle(family);

                //bool symfont = IsSymbolFont(name);

                //determinate physical size, so that we can have captial H to be correct pixel height
                using (GraphicsPath fpth = new GraphicsPath())
                {
                    string measurestring = "H";
                    /*if (symfont)
                        measurestring = "ABCDEFGIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";    //for symbol fonts, capital E means nothing, we take a chance and measure entire alphabets size*/



                    float size = 1f;
                    fpth.AddString(measurestring, family, (int)fontstyle, size, Point.Empty, StringFormat.GenericTypographic);
                    var bounds = fpth.GetBounds();
                    size = (size / bounds.Height) * size;
                    font = new Font(name, size, fontstyle, GraphicsUnit.Pixel);
                }



                double em = (double)family.GetEmHeight(fontstyle) / font.Size;
                double linespace_pixels = (double)family.GetLineSpacing(fontstyle) / em;
                double ascent_pixels = (double)family.GetCellAscent(fontstyle) / em;
                double descent_pixels = (double)family.GetCellDescent(fontstyle) / em;




                res = new CachedFont(font, linespace_pixels, ascent_pixels, descent_pixels);
                _fontCache[name] = res;
                return res;
            }
            catch
            {
                return null;
            }

        }
        

        private void ComputeTextAlign(CachedFont cf,TextAlign align, string text, double size, out double dx, out double dy)
        {
            dx = dy = 0.0;
            double strw, strh;

            //assumes gdifont:s unit is in pixels
            /*double em = (double)_gdiFont.FontFamily.GetEmHeight(FontStyle.Regular);
            double pixsiz=_gdiFont.Size; //GDI size spec. of font in pixels
            double linespace_pixels = (double)_gdiFont.FontFamily.GetLineSpacing(FontStyle.Regular) / em * pixsiz;
            double ascent_pixels = (double)_gdiFont.FontFamily.GetCellAscent(FontStyle.Regular) /em * pixsiz;*/
            

            //compute y
            if ((align & TextAlignAtom.TEXT_TOP) != 0)
                dy = 0.0;
            else if ((align & TextAlignAtom.TEXT_CENTER_Y) != 0)
            {
                dy = (-cf.LineSpacingPixels / 2.0) * size;
            }
            else if ((align & TextAlignAtom.TEXT_BASE) != 0)
            {
                dy = -cf.AscentPixels * size;
            }
            else
            { //bottom
                dy = -cf.LineSpacingPixels * size;
            }

            //compute x
            if ((align & TextAlignAtom.TEXT_LEFT) != 0)
                dx = 0.0;
            else if ((align & TextAlignAtom.TEXT_CENTER_X) != 0)
            {
                GetTextSize(text, size, out strw, out strh);
                dx = -strw / 2.0;
            }
            else if ((align & TextAlignAtom.TEXT_RIGHT) != 0)
            {
                GetTextSize(text, size, out strw, out strh);
                dx = -strw;
            }
        }


        private Transform2d _getTextTransform(CachedFont cf,string txt, double x, double y, double size, double angle, TextAlign align, Transform2d t)
        {
            //computes the transformation needed to transform text drawn at origo to get to the wanted position
            Transform2d res = Transform2d.Translate(x, y);
            if (angle != 0.0) res = Transform2d.Rotate(angle) * res;
            res = Transform2d.Stretch(1.0, -1.0) * res; //flipped coordinate system

            if (align != TextAlign.TopLeft)
            {
                double dx, dy;
                ComputeTextAlign(cf,align, txt, size, out dx, out dy);
                res = Transform2d.Translate(dx, dy) * res;
            }

            res = Transform2d.Scale(size) * res;

            if (t != null)
                return res * t;
            return res;
        }



        /// <summary>
        /// Draws text using the current font and the given parameters. Returns true on success, or
        /// false on failure, in which case base class font renderer should be called.
        /// </summary>
        bool InternalDrawText(CachedFont cf,string txt, double x, double y, double size, double angle, TextAlign align, Transform2d t)
        {
           


            if (size < MathUtil.Epsilon)  //MultiplyTransform cant manage zero length axes of matrices
            {
                SmallPrimitive(x, y);
                return true;
            }

            UpdateLineBrush();

            var oldtrans = gfx.Transform;
            var tm = _getTextTransform(cf, txt, x, y, size, angle, align, t);
            gfx.MultiplyTransform(TransformToMatrix(tm));

            gfx.DrawString(txt, cf.Font, linebrush, 0f, 0f, StringFormat.GenericTypographic);
            gfx.Transform = oldtrans;
            return true;

        }

        public override void DrawTextT(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            CachedFont cf = FindOrCreateFont(Font);
            if (cf == null)
            {
                base.DrawTextT(txt, x, y, size, angle, align);
                return;
            }
            InternalDrawText(cf,txt, x, y, size, angle, align, Transform); //TODO: use base class font renderer on failure
        }

        public override void DrawText( string txt, double x, double y, double size, double angle, TextAlign align)
        {
            CachedFont cf = FindOrCreateFont(Font);
            if (cf == null)
            {
                base.DrawText(txt, x, y, size, angle, align);
                return;
            }
            InternalDrawText(cf,txt, x, y, size, angle, align, null); //TODO: use baseclass font renderer on failure
        }



        static StringFormat _measureformat = null;
        private StringFormat MeasureStringFormat
        {
            get
            {
                //GenericTypographic with MeasureTrailingSpaces added
                if (_measureformat == null)
                {
                    _measureformat = new StringFormat(StringFormat.GenericTypographic);
                    _measureformat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
                }
                return _measureformat;
            }
        }


        public override GFXPath GetTextPath(string txt, double x, double y, double size, double angle, TextAlign align, Transform2d t)
        {
            CachedFont cf=FindOrCreateFont(Font);
            if (cf == null)
            {
                return base.GetTextPath(txt, x, y, size, angle, align, t);
            }
             

            Transform2d tt = _getTextTransform(cf, txt, x, y, size, angle, align, t);
            GraphicsPath pth = new GraphicsPath();
            pth.AddString(txt, cf.Font.FontFamily, (int)cf.Font.Style, cf.Font.Size, new PointF(0, 0f), StringFormat.GenericTypographic);
            pth.Transform(PainterGDIPlus.TransformToMatrix(tt));
            return GetGFXPath(pth);
        }


        public override double[] GetTextBounds(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            double w, h; 

            CachedFont cf = FindOrCreateFont(Font);
            if (cf == null)
                return base.GetTextBounds(txt, x, y, size, angle, align);
             

            GetTextSize(txt, 1.0, out w, out h); //we scale text up with matrix later, thus size 1.0
            var t = _getTextTransform(cf, txt, x, y, size, angle, align, Transform2d.Identity);

            double[] res = new double[8] { 0, h, w, h, w, 0, 0, 0 };
            t.Apply(res[0], res[1], out res[0], out res[1], true);
            t.Apply(res[2], res[3], out res[2], out res[3], true);
            t.Apply(res[4], res[5], out res[4], out res[5], true);
            t.Apply(res[6], res[7], out res[6], out res[7], true);

            return res;
        }

      


        void InternalGetTextSize(CachedFont cf, string txt, double size, out double w, out double h)
        {
            if (cf == null) { w = h = 0.0; return; }
            SizeF sf = gfx.MeasureString(txt, cf.Font, int.MaxValue, MeasureStringFormat);
            w = (double)sf.Width * size;
            h = (double)sf.Height * size;
        }


        public override void GetTextSize(string txt, double size, out double w, out double h)
        {
            CachedFont cf=FindOrCreateFont(Font);
            if (cf == null)
            {
                base.GetTextSize(txt, size, out w, out h);
                return;
            }

            InternalGetTextSize(cf, txt, size,out w,out h);
        }


        public override void GetFontDim(double size, out double linespace_pixels, out double ascent_pixels, out double descent_pixels,out bool filled)
        {
            

            CachedFont cf=FindOrCreateFont(Font);
            if (cf == null)
            {
                base.GetFontDim(size,out linespace_pixels,out ascent_pixels,out descent_pixels,out filled);
                return;
            }

            //multiply fonts size to get the actual number of pixels
            ascent_pixels = cf.AscentPixels * size;
            descent_pixels = cf.DescentPixels * size;
            linespace_pixels = cf.LineSpacingPixels * size;
            filled = true; //true type text is always filled
        }
        
        #endregion


        #region UTILS

        private void UpdateLinePen()
        {
            if (linepen != null)
                return; //already have a pen

            linepen = PenGenerator.CreateLinePenFromPainter(this);
        }

        private void UpdateLineBrush()
        {
            if (linebrush != null)
                return; //already have brush

            linebrush = new SolidBrush(ApplyOpacity(fgcolor));
        }

        private void UpdateBrush(Transform2d patterntrans)
        {
            //update transform if such given
            if (Pattern != null && FillStyle == FillStyle.Pattern)
            {
                var tb = MapPattern(Pattern);
                if (tb != null)
                {
                    if (patterntrans != null)
                    {
                        tb.Transform = TransformToMatrix(Transform2d.Stretch(1, -1) * Transform);
                    }
                    else
                    {
                        tb.Transform = TransformToMatrix(Transform2d.Stretch(1, -1));
                    }

                    if (!PatternTransform.IsIdentity)
                    {
                        var m = tb.Transform;
                        m.Multiply(TransformToMatrix(PatternTransform), MatrixOrder.Append);
                        tb.Transform = m;
                    }
                }
            }

            if (fillbrush != null)
                return; //already have a brush

            if (FillStyle == FillStyle.Hatch)
            {
                fillbrush = new HatchBrush(HatchToGDIHatchStyle(Hatch), ApplyOpacity(fgcolor), ApplyOpacity(bgcolor));
            }
            else if (FillStyle == FillStyle.Pattern && Pattern is Picture)
            {
                fillbrush = MapPattern(Pattern);
            }
            else //Solid
            {
                fillbrush = new SolidBrush(ApplyOpacity(fgcolor));
            }
        }

        private System.Drawing.Color ApplyOpacity(System.Drawing.Color col)
        {
            if (Opacity < 0.999)
            {
                int alpha = GFXUtil.FloatToInt(MathUtil.Clamp(Opacity * 255.0, 0.0, 255.0));
                col = System.Drawing.Color.FromArgb(alpha, col);
            }

            return col;
        }

        private HatchStyle HatchToGDIHatchStyle(Hatch Hatch)
        {
            switch (Hatch)
            {
                case Hatch.BackwardDiagonal: return HatchStyle.BackwardDiagonal;
                case Hatch.ForwardDiagonal: return HatchStyle.ForwardDiagonal;
                case Hatch.Cross: return HatchStyle.Cross;
                case Hatch.DiagonalCross: return HatchStyle.DiagonalCross;
                case Hatch.Horizontal: return HatchStyle.Horizontal;
                case Hatch.Vertical: return HatchStyle.Vertical;
                case Hatch.Checkers: return HatchStyle.LargeCheckerBoard;
                default: return HatchStyle.Cross;
            }
        }

        protected void ReleaseObjects(bool release_lineobjects, bool release_fillobjects)
        {
            if (release_lineobjects)
            {
                if (linepen != null)
                {
                    linepen.Dispose();
                    linepen = null;
                }

                if (linebrush != null)
                {
                    linebrush.Dispose();
                    linebrush = null;
                }
            }

            if (release_fillobjects && fillbrush != null)
            {
                if (!(fillbrush is TextureBrush)) //we dont release texture brushes since they come from ServerImage which user is responsible for freeing
                    fillbrush.Dispose();
                fillbrush = null;
            }
        }

        internal static GraphicsPath GetGraphicsPath(GFXPath path)
        {
            //we cache GDIPlus graphics path in cache object 1 of path
            GraphicsPath res = path.CacheObject1 as GraphicsPath;
            if (res != null)
                return res;

            res = new GraphicsPath();

            double penx=0.0,peny=0.0;

            foreach(GFXPathMoveTo node in path.PathPoints) {

                if (node is GFXPathLineTo)
                    res.AddLine((float)penx, (float)peny, (float)node.X, (float)node.Y);
                else if (node is GFXPathArcTo)
                {
                    GFXPathArcTo a = node as GFXPathArcTo;
                    double cx, cy, startang, sweepang, rad;
                    if (GeomUtil.GetArcParams(penx, peny, a.X, a.Y, a.Bulge, out cx, out cy, out rad, out startang, out sweepang))
                    {
                        res.AddArc((float)(cx - rad), (float)(cy - rad), (float)(rad * 2.0), (float)(rad * 2.0), (float)MathUtil.RadToDeg(startang), (float)MathUtil.RadToDeg(sweepang));
                    }
                    else
                        res.AddLine((float)penx, (float)peny, (float)a.X, (float)a.Y);
                }
                else if (node is GFXPathBezierTo)
                {
                    GFXPathBezierTo bez = node as GFXPathBezierTo;
                    res.AddBezier((float)penx, (float)peny, (float)bez.XC1, (float)bez.YC1, (float)bez.XC2, (float)bez.YC2, (float)bez.X, (float)bez.Y);
                }
                else if (node is GFXPathCloseSubPath)
                {
                    res.CloseFigure();
                }
                else
                { //assume move to
                    res.StartFigure();
                }
                

                penx = node.X;
                peny = node.Y;
            }

            path.CacheObject1 = res;

            return res;
        }


        /// <summary>
        /// Computes a GFXPath from the graphics path, storing the GraphicsPath itself as CacheObject1
        /// </summary>
        /// <param name="path"></param>
        internal static GFXPath GetGFXPath(GraphicsPath path)
        {
            
            GraphicsPathIterator it = new GraphicsPathIterator(path);
            GFXPath res=new GFXPath();

            int n = path.PointCount;
            if (n < 2)
            {
                return res;
            }

            var pts = path.PathPoints;
            var ptyp = path.PathTypes;
            

            for (int l = 0; l < n; l++)
            {

                int typ = ptyp[l];

                int segtyp = typ & 3;

                if (segtyp == 0)
                    res.MoveTo((double)pts[l].X, (double)pts[l].Y);
                else if (segtyp == 1)
                    res.LineTo((double)pts[l].X, (double)pts[l].Y);
                else if (segtyp == 3)
                {
                    res.BezierTo(pts[l].X, pts[l].Y, pts[l + 1].X, pts[l + 1].Y, pts[l + 2].X, pts[l + 2].Y);
                    l += 2; //jump over used controlpoints
                }

                if (ptyp[l] >= 128)   //close
                    res.CloseSubPath();
            }

            res.CacheObject1 = path;

            return res;
        }
        

        public override void Dispose()
        {
            ReleaseObjects(true, true);

            if (hdc != IntPtr.Zero)
            {
                GDI.DeleteDC(hdc);
                hdc = IntPtr.Zero;
            }

            //release our internal pattern if needed
            if (_ownsGDIPattern && _gdiPattern != null) //release old pattern if we owns it
                _gdiPattern.Dispose();


            if (oldgfxstate == null) //we owns graphic object
                gfx.Dispose();
            else //we dont own graphics object, restore old state for it
                gfx.Restore(oldgfxstate);
        }

        private static bool IsSymbolFont(string fontname) //TODO: is this func. used??
        {
            try
            {
                using (Font f = new Font(fontname,100f))
                {

                    TEXTMETRIC tm;

                    IntPtr dc = GDI.CreateCompatibleDC(IntPtr.Zero);
                    IntPtr hfont = f.ToHfont();
                    GDI.SelectObject(dc, hfont);
                    GDI.GetTextMetrics(dc, out tm);
                    GDI.DeleteObject(hfont);
                    GDI.DeleteDC(dc);
                    return tm.tmCharSet == 2;
                }
            }
            catch
            {
                return false; //somethign went wrong, probably invalid font, assume not a symbol font
            }
        }

#endregion


        #region MISC

        public override void Clear(int rgb)
        {
            Color bgcolor;
            if(rgb<0)
                bgcolor=System.Drawing.Color.Transparent;
            else
                bgcolor = System.Drawing.Color.FromArgb(RGB.GetRed(rgb), RGB.GetGreen(rgb), RGB.GetBlue(rgb));

            Region r = gfx.Clip;
            gfx.ResetClip(); //clear should not use clipping
            gfx.Clear(bgcolor);
            gfx.Clip = r;
        }

        public override void MillimetersToPixels(double mmx, double mmy, out double pixx, out double pixy)
        {
            pixx = mmx * pixels_per_mm_x;
            pixy = mmy * pixels_per_mm_y;
        }

        public override int Width
        {
            get { return width; }
        }

        public override int Height
        {
            get { return height; }
        }

        #endregion

    }

    

  

}
