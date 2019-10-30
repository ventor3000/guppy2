using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
   

    [Flags]
    public enum TypefaceStyle {
        Regular=0,
        Bold=1,
        Italic=2,
        Underline=4 
    }
    

    public class Typeface:GUIObject
    {
        IDriverGUIObject driverface;
        public readonly double Size;
        public readonly TypefaceStyle Style;
        
        public Typeface(string facename,double size,TypefaceStyle style=TypefaceStyle.Regular) {
            Construct(Guppy.Driver.CreateTypeface(facename,size,style));
        }
    }
}
