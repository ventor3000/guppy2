using Guppy2.GUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Guppy2.GFX.Windows
{
    public unsafe class PictureWinRGB : Picture,IPixelAccess
    {
        protected IntPtr hbm = IntPtr.Zero;
        internal int* pixels; //this is where we draw, raw pointer to it.
        internal int*[] scanlines; //lut for scanlines
        private int width, height;
        bool _disposed = false;


        ~PictureWinRGB()
        {
            Dispose(false);
        }


        

        public PictureWinRGB(int width, int height)
        {
            Create(width, height);
        }

        public PictureWinRGB(Stream stream)
        {
            using (var bmp = System.Drawing.Bitmap.FromStream(stream))
            {
                Create(bmp.Width, bmp.Height);
                SoftPicture.CopyBitmapToPixelAccess(bmp as System.Drawing.Bitmap, this, false);
            }
        }

        private void Create(int width, int height)
        {
            this.width = width;
            this.height = height;

            if (width < 1) width = 1;
            if (height < 1) height = 1;

            BITMAPINFO bi = new BITMAPINFO();
            bi.biSize = (UInt32)Marshal.SizeOf(bi);
            bi.biBitCount = 32;
            bi.biClrUsed = 0;
            bi.biClrImportant = 0;
            bi.biCompression = 0;
            bi.biHeight = height;
            bi.biWidth = width;
            bi.biPlanes = 1;
            bi.biSizeImage = 0;

            IntPtr bits = IntPtr.Zero;

            hbm = GDI.CreateDIBSection(IntPtr.Zero, ref bi, 0, out bits, IntPtr.Zero, 0);

            //check gdi failure
            if (hbm == IntPtr.Zero)
                throw new Exception("Failed to create PictureWinRGB");

            // GDI.SelectObject(ReferenceHDC, hbm); //our bitmap is the active in the hdc

            pixels = (int*)bits;

            //pre-compute scanlines
            int* scan = pixels;
            scanlines = new int*[height];
            for (int l = 0; l < height; l++)
            {
                scanlines[l] = scan;
                scan += width;
            }
        }

        public override int Width
        {
            get { return width; }
        }

        public override int Height
        {
            get { return height; }
        }

        public IntPtr Pixels
        {
            get
            {
                return new IntPtr(pixels);
            }
        }

        public override SoftPicture ToSoftPicture()
        {
            SoftPicture sp = new SoftPicture(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    sp.UnsafeSetPixel(x, y, UnsafeGetPixel(x, y));
                }
            }
            return sp;
        }


        public int UnsafeGetPixel(int x, int y)
        {
            return *(scanlines[y] + x);
        }

        public void UnsafeSetPixel(int x, int y, int color)
        {
            *(scanlines[y] + x) = color;
        }


        public int GetPixel(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return 0;
            return *(scanlines[y] + x);
        }

        public void SetPixel(int x, int y, int color)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;
            *(scanlines[y] + x) = color;
        }


        public override void Dispose()
        {
            Dispose(true);
        }


        protected virtual void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these  
            // operations, as well as in your methods that use the resource. 
            if (!_disposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                }

                //Dispose unmanaged resources
                if (hbm != IntPtr.Zero)
                {
                    GDI.DeleteObject(hbm);
                    hbm = IntPtr.Zero;
                }

                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }

        /// <summary>
        /// Gets the HBITMAP of the dib section
        /// </summary>
        public IntPtr Handle { get { return hbm; } }


        public override object NativeObject
        {
            get { return hbm; }
        }

        public override Painter CreatePainter()
        {
            return new PainterWinRGB(this);
        }
    }
}
