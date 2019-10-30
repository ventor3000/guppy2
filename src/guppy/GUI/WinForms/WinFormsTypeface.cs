using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF=System.Windows.Forms;
using SD=System.Drawing;

namespace Guppy2.GUI.WinForms
{


    public class WinFormsTypeface:IDriverTypeface
    {
        public readonly SD.Font Font;
        bool ownsfont;

        ~WinFormsTypeface()
        {
            Dispose();
        }

        public WinFormsTypeface(string name, double size, TypefaceStyle style)
        {
            SD.FontStyle sdfs=SD.FontStyle.Regular;
            if( (style&TypefaceStyle.Bold)!=0) sdfs|=SD.FontStyle.Bold;
            if( (style&TypefaceStyle.Italic)!=0) sdfs|=SD.FontStyle.Italic;
            if( (style&TypefaceStyle.Underline)!=0) sdfs|=SD.FontStyle.Underline;

            Font=new SD.Font(name,(float)size,sdfs);
            ownsfont = true;
        }

        public void Dispose()
        {
            if (Font != null && ownsfont)
                Font.Dispose();
        }

        public object NativeObject
        {
            get { return Font; }
        }
    }
}
