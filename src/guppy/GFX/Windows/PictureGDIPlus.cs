using Guppy2.Calc;
using Guppy2.GUI;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Guppy2.GFX.Windows
{
    public class PictureGDIPlus : Picture
    {
        System.Drawing.Image img;
        TextureBrush tb = null; //cached texture brush, created on demand
        double textureopacity;   //opacity of the current texture

        readonly bool ownsimage;

        public PictureGDIPlus(System.Drawing.Image img)
        {
            this.img = img;
            ownsimage = false;
        }

        public PictureGDIPlus(Stream stream)
        {
            this.img = Bitmap.FromStream(stream);
            ownsimage = true;
        }
            

        public PictureGDIPlus(int w, int h)
        {
            this.img = new Bitmap(w, h);
            ownsimage = true;
        }

        public PictureGDIPlus(SoftPicture softimg)
        {
            img = new Bitmap(softimg.Width, softimg.Height);
            ownsimage = true;
            CopyFromSoftPicture(softimg);
        }

        public void CopyFromSoftPicture(SoftPicture softimg)
        {

            Bitmap bmp = img as Bitmap;
            if (bmp == null)
                return;

            int w = Math.Min(bmp.Width, softimg.Width);
            int h = Math.Min(bmp.Height, softimg.Height);

            BitmapData bdat = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            int bytesperrow = bdat.Stride;
            int bytesperpixel = 4;
            IntPtr scan0 = bdat.Scan0;

            int srcy = softimg.Height - 1; //softimage is stored opposite y direction than GDI image

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int col = RGB.InvertAlpha(softimg.GetPixel(x, srcy));
                    Marshal.WriteInt32(scan0, x * bytesperpixel + y * bytesperrow, col);
                }

                srcy--;
            }

            bmp.UnlockBits(bdat);

        }

        public void CopyToSoftPicture(SoftPicture softpic)
        {
            Bitmap bmp = img as Bitmap;
            if (bmp == null)
                return;

            int w = Math.Min(img.Width, bmp.Width);
            int h = Math.Min(img.Height, bmp.Height);

            BitmapData bdat = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            int bytesperrow = bdat.Stride;
            int pixelsperrow = bytesperrow / 4;
            IntPtr scan0 = bdat.Scan0;

            int srcy = softpic.Height - 1; //iverted y in soft picture

            unsafe
            {
                int* pixels = (int*)scan0.ToPointer();

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int col = pixels[x + y * pixelsperrow] & 0xffffff;
                        softpic.SetPixel(x, srcy, col);
                    }

                    srcy--;
                }
            }

            bmp.UnlockBits(bdat);
        }

        public override int Width
        {
            get
            {
                return img.Width;
            }
        }

        public override int Height
        {
            get
            {
                return img.Height;
            }
        }

        public TextureBrush GetTextureBrush(double opacity)
        {

            opacity = MathUtil.Clamp(opacity, 0.0, 1.0);

            //get rid of old cahed texture texture if existing and wron opacity
            if (tb != null && !MathUtil.Equals(opacity, textureopacity, 0.001))
            {
                tb.Dispose();
                tb = null;
            }

            if (tb == null) //not previously cached, create a new one
            {

                if (opacity < 0.999) //transparent texture
                {
                    // Even thu the API to create a texture brush with an ColorMatrix, transparency of
                    // texture brushes is not supported ( from direct answer from a .NET developer, see 
                    // http://www.databaseforum.info/21/1010445.aspx ). We solve this by creating a transparent version
                    // of the input image and creating a texture brush with this image instead:
                    Bitmap bmp = new Bitmap(img.Width, img.Height);
                    using (Graphics gr = Graphics.FromImage(bmp))
                    {
                        gr.Clear(Color.Transparent);

                        ColorMatrix cm = new ColorMatrix();
                        cm.Matrix33 = (float)opacity;
                        ImageAttributes attr = new ImageAttributes();
                        attr.SetColorMatrix(cm);
                        gr.DrawImage(img,
                            new Rectangle(0, 0, img.Width, img.Height),
                            0f, 0f, (float)img.Width, (float)img.Height, GraphicsUnit.Pixel, attr);
                    }

                    tb = new TextureBrush(bmp);
                    textureopacity = opacity;

                }
                else //solid texture
                {
                    System.Drawing.Image ni = new Bitmap(img); //work around "out of memory" bug (?) in gdi+
                    tb = new TextureBrush(ni);
                    textureopacity = 1.0;
                }
            }

            return tb;

        }

        public System.Drawing.Image Image
        {

            get
            {
                return img;
            }
        }



        public void UncacheTexture()
        { //need to be called when painting to this image becase we cant paint directly to texture in GDI+ 
            if (tb != null)
            {
                tb.Dispose();
                tb = null;
            }
        }

        public override void Dispose()
        {
            if (img != null && ownsimage)
            {
                img.Dispose();
                img = null;
            }
            UncacheTexture();
        }

        public override SoftPicture ToSoftPicture()
        {
            SoftPicture cli = new SoftPicture(Width, Height);
            CopyToSoftPicture(cli);
            return cli;
        }

        public override object NativeObject //implement IDriverImage
        {
            get { return img; }
        }

        public override Painter CreatePainter()
        {
            return new PainterGDIPlus(this);
        }
    }
}
