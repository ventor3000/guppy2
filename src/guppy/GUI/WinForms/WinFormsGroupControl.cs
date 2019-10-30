using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Guppy2.GUI.WinForms
{
    public class WinFormsGroupControl:WinFormsControl
    {

        public override Size2i NaturalSize
        {
            get
            {
                return Size2i.Natural; //Natural size for group controls uses default size calculation
            }
        }

        virtual public void Detach(IDriverWidget child)
        {
            WinFormsControl c = child as WinFormsControl;
            if (c == null || c.Control == null) return;
            if (Control.Contains(c.Control))
                Control.Controls.Remove(c.Control);
        }

        virtual public void Append(IDriverWidget child)
        {
            WinFormsControl c = child as WinFormsControl;
            if (c == null || c.Control == null) return;
            Control.Controls.Add(c.Control);
        }


        virtual public Point2i Origin
        {
            get
            {
                return Point2i.Origin;
            }
        }

    }
}
