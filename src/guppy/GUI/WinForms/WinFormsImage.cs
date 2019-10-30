using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SD=System.Drawing;
using System.IO;

namespace Guppy2.GUI.WinForms
{
    public class WinFormsImage:IDriverImage
    {
        SD.Image image=null;

        public WinFormsImage(Stream src) {
            image=SD.Image.FromStream(src);
        }

        public int Width { get { return image.Width; } }
        public int Height { get { return image.Height; } }
        public void Dispose() {if (image != null) image.Dispose();}

        public object NativeObject { get { return image; } }

    }
}
