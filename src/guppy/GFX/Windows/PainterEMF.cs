using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GFX.Windows
{
    public class PainterEMF:PainterGDIPlus
    {
        PictureEMF target = null;

        public PainterEMF(PictureEMF target):base(false)
        {
            this.target = target;
            Init(target.Begin());
        }


        public override void Dispose()
        {

            base.Dispose();

            if (target != null)
            {
                target.End();
                target = null;
            }
        }

        public override void Clear(int rgb)
        {
            
            //exchange graphics object for out new
            //also restore properties of graphic that we use
            var oldtrans = gfx.Transform;
            var oldsmooth = gfx.SmoothingMode;
            var oldinterpol = gfx.InterpolationMode;
            var oldclip = gfx.Clip;

            ReleaseObjects(true, true);

            gfx=target.Recreate();
            gfx.Transform = oldtrans;
            gfx.SmoothingMode = oldsmooth;
            gfx.InterpolationMode = oldinterpol;
            


            //to clear a metafile, we recreate it, and fill it with a rectangle of given color
            //unless the color is fully transparent
            if (rgb>=0)
            { //not transparent
                int oldcol = Color;
                Color=rgb;
                FillRectangle(0, 0,target.Width - 1, target.Height - 1);
                Color = oldcol;
            }

            gfx.Clip = oldclip; //restore clipping rectangle after clear-rectangle is drawn since clear does not obey clipping
        }

        public override int Width
        {
            get
            {
                return target.Width;
            }
        }

        public override int Height
        {
            get
            {
                return target.Height;
            }
        }
    }
}
