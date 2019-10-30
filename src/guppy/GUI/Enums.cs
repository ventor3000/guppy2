using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    
    public enum PackSide
    {
        //Flags used by the SizerPack sizer
        Top=1,
        Left=2,
        Right=3,
        Bottom=4
    }

   
    public enum Align
    {
        Center = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,

        TopLeft = Top | Left,
        TopRight = Top | Right,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right
    }

    

}
