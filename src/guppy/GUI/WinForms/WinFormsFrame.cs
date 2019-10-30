using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WF = System.Windows.Forms;

namespace Guppy2.GUI.WinForms
{
    public class WinFormsFrame:WinFormsGroupControl,IDriverFrame
    {
        public WinFormsFrame(GUIObject guiobject, string caption)
        {
            Init(new WF.GroupBox(), guiobject);
            GroupBox.Text = caption;
        }

        private WF.GroupBox GroupBox { get { return Control as WF.GroupBox; } }

        public Margin BorderDecorMargin
        {
            get
            {
                var r=GroupBox.DisplayRectangle;
                return new Margin(
                    r.Left,
                    r.Top,
                    Control.Width - r.Right,
                    Control.Height - r.Bottom
                );
            }
        }

        public override Point2i Origin
        {
            get
            {
                var r=GroupBox.DisplayRectangle;
                return new Point2i(r.Left, r.Top);
            }
        }

        

    }
}
