using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF = System.Windows.Forms;

namespace Guppy2.GUI.WinForms
{
    public class WinFormsSeparator:WinFormsControl,IDriverSeparator
    {

        const int thickness = 3;
        bool vertical;

        public WinFormsSeparator(GUIObject owner, bool vertical)
        {
            this.vertical = vertical;

            
            Init(new WF.Label(), owner);

            Label.AutoSize = false;
            Label.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            if (vertical)
            {
                Label.Width = thickness;
                Label.Height = 16;
            }
            else
            {
                Label.Height = thickness;
                Label.Width = 16;
            }


        }

        public override Size2i NaturalSize
        {
            get
            {
                if (vertical)
                    return new Size2i(thickness,16);
                else
                    return new Size2i(16, thickness);
            }
        }

        private WF.Label Label { get { return Control as WF.Label;}}
    }
}
