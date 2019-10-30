using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Guppy2.GFX
{

    public interface IPixelAccess
    {
        void SetPixel(int x, int y, int rgb);
        int GetPixel(int x, int y);
        void UnsafeSetPixel(int x, int y, int rgb);
        int UnsafeGetPixel(int x, int y);
    }



    /// <summary>
    /// This class represents a software representation of an image. It stores the pixels of the image
    /// bottom-up, that is pixel 0,0 is lower left of image.
    /// </summary>
    public class SoftPicture:Picture,IPixelAccess
    {
        public int[] argb;
        int w, h;

        public SoftPicture(int width, int height)
        {
            this.w = width;
            this.h = height;
            argb = new int[width*height];
        }

        public override int Width { get { return w; } }
        public override int Height { get { return h; } }

        public void SetPixel(int x, int y, int color)
        {
            if (x < 0 || x >= w || y < 0 || y >= h) return;
            argb[x+y*w] = color;
        }

        public void UnsafeSetPixel(int x, int y, int color)
        {
            argb[x + y * w] = color;
        }

        public int GetPixel(int x, int y)
        {
            if (x < 0 || x >= w || y < 0 || y >= h) return 0;
            return argb[x+y*w];
        }

        public int UnsafeGetPixel(int x, int y)
        {
            return argb[x + y * w];
        }

        public override SoftPicture ToSoftPicture()
        {
            return this;    //we already are a soft picture
        }


        public int[] Pixels
        {
            get
            {
                return argb;
            }
        }
        
        public override void Dispose()
        {
           
        }

        public static SoftPicture FromBitmap(Bitmap bmp)
        {
            if (bmp == null)
                throw new Exception("Invalid bitmap data");

            int width = bmp.Width;
            int height = bmp.Height;

            SoftPicture res = new SoftPicture(width, height);

            CopyBitmapToPixelAccess(bmp, res, true);

            return res;
            
            /*
            BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = bmpdata.Stride / 4;
            IntPtr pixels = bmpdata.Scan0;
            //int pixelpos = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                   

                    //our soft picture is stored 'bottom up' that is pixel 0,0 is lower left, wo we
                    //flip pixel data in y direction
                    int color = (int)Marshal.ReadInt32(pixels, (x + y * stride) * 4);
                    res.UnsafeSetPixel(x, res.Height - y - 1, RGB.InvertAlpha(color));

                }
            }

            bmp.UnlockBits(bmpdata);

            return res;*/
        }

        internal static void CopyBitmapToPixelAccess(Bitmap bmp,IPixelAccess pa,bool invertalpha) {

            //note PA:s size HAS to be the same size as bmp:s size! (we use UnsafeSetPixel:s)

            if (bmp == null)
                throw new Exception("Invalid bitmap data");

            int width = bmp.Width;
            int height = bmp.Height;

            
            BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = bmpdata.Stride / 4;
            IntPtr pixels = bmpdata.Scan0;
            //int pixelpos = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    /*int color = (int)Marshal.ReadInt32(pixels, (x + y * stride) * 4);
                    res.argb[pixelpos++] = GFXUtil.InvertAlpha(color);*/


                    //our soft picture is stored 'bottom up' that is pixel 0,0 is lower left, wo we
                    //flip pixel data in y direction
                    int color = (int)Marshal.ReadInt32(pixels, (x + y * stride) * 4);
                    pa.UnsafeSetPixel(x, height - y - 1, invertalpha ? RGB.InvertAlpha(color):color);

                }
            }

            bmp.UnlockBits(bmpdata);
        }
        
        ///generators
        public static SoftPicture FromStream(Stream str)
        {
            return FromBitmap(Bitmap.FromStream(str) as Bitmap);
        }

        public static SoftPicture FromFile(string filename)
        {
            return FromBitmap(Bitmap.FromFile(filename) as Bitmap);
        }

        public SoftPicture ToGrayScale()
        {

            SoftPicture res = new SoftPicture(w, h);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    res.UnsafeSetPixel(x,y,RGB.ToGray(UnsafeGetPixel(x,y)));
                }
            }

            return res;
        }

        public override object NativeObject
        {
            get { return argb; }
        }

        public override Painter CreatePainter()
        {
            return null; //TODO: painter for soft pictures? keep soft pictures at all?
        }
    }
}
