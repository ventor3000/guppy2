using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2Test
{

    public enum ColorMode
    {
        ByColor,
        ByLayer,
        ByBlock
    }

    public abstract class EntitySpatial:Entity
    {
        public ColorMode ColorMode = ColorMode.ByColor;
        public int Color = RGB.White;
        public bool Selected = false;


        public int PhysicalColor
        {
            get
            {
                switch (ColorMode)
                {
                    case ColorMode.ByColor: return Color;
                    case ColorMode.ByBlock: return RGB.White;   //TODO: use correct color
                    case ColorMode.ByLayer: return RGB.White;   //TODO: use correct color
                    default: return RGB.Red;    //Error: this should never happen
                }
            }
        }
                        


        public abstract void Draw(Painter painter);
        public virtual void DrawSelected(Painter painter) { Draw(painter); }
    }
}
