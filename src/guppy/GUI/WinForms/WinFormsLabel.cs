using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF=System.Windows.Forms;

namespace Guppy2.GUI.WinForms
{
    public class WinFormsLabel : WinFormsControl, IDriverLabel
    {
        public WinFormsLabel(GUIObject guiobject,string caption)
        {
            Init(new WF.Label(), guiobject);
            Label.Text = caption ?? "";
            //Label.BackColor = System.Drawing.Color.DarkCyan;
            Label.AutoSize = false;
        }
        

        private WF.Label Label
        {
            get
            {
                return Control as WF.Label;
            }
        }

        public override Size2i NaturalSize
        {
            get
            {
                var ws = Label.PreferredSize;
                return new Size2i(ws.Width, ws.Height);
            }
        }
    }
}
