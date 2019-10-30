using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Guppy2.GFX.Windows
{
    public class PictureEMF : Picture
    {

        MemoryStream memorystream; //this is the actual contents of the metafile
        int width, height;
        Metafile metafile = null;
        Graphics currentgraphics = null;    //if not null, we're between an Begin/End


        public byte[] Data
        {
            get
            {
                return memorystream.ToArray();
            }
        }
        public PictureEMF(int width, int height)
        {
            this.width = width;
            this.height = height;
            RecreateInternalMetafile(width, height);
        }


        internal Graphics Begin()
        {
            currentgraphics=Graphics.FromImage(metafile);
            return currentgraphics;
        }


        internal void End()
        {
            if (currentgraphics != null)
            {
                currentgraphics.Dispose();
                currentgraphics = null;
            }
        }


        private void RecreateInternalMetafile(int width,int height)
        {
            memorystream = new MemoryStream();
            Graphics offscreengfx = Graphics.FromHwndInternal(IntPtr.Zero);
            IntPtr hdc = offscreengfx.GetHdc();
            metafile = new Metafile(memorystream, hdc, new Rectangle(0, -height + 1, width, height), MetafileFrameUnit.Pixel, EmfType.EmfOnly);

            offscreengfx.ReleaseHdc();
            offscreengfx.Dispose();
            offscreengfx = null;
        }


        /// <summary>
        /// Empties the metafile and returns a new graphis (if between a Begin/End), or null if drawing state is not
        /// active.
        /// </summary>
        /// <returns></returns>
        internal Graphics Recreate()
        {
            if (currentgraphics != null)
            {
                //we are currently active, gotta end
                End();
                RecreateInternalMetafile(width, height);
                return Begin();
            }
            else
            {
                Recreate();
                return null;
            }
        }


        public void Save(string filename)
        {

            if (memorystream != null)
            {
                memorystream.Position = 0;
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    memorystream.WriteTo(fs);
                    fs.Close();
                }
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

        public override SoftPicture ToSoftPicture()
        {
            //TODO: implement (possible? blit to raster image?)
            return null;
        }
        
        

        public override void Dispose()
        {
            if (metafile != null)
            {
                metafile.Dispose();
                metafile = null;
            }

            if (memorystream != null)
            {
                memorystream.Dispose();
            }
            
        }

        public override object NativeObject
        {
            get { return memorystream; }
        }

        public override Painter CreatePainter()
        {
            return new PainterEMF(this);
        }
    }

}
