using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WF = System.Windows.Forms;

namespace Guppy2.GUI.WinForms
{
    public class WinFormsToggle:WinFormsControl,IDriverToggle
    {

        public WinFormsToggle(GUIObject guiobject, string caption)
        {
            Init(new WF.CheckBox(), guiobject);
            Caption = caption ?? "";
        }

        public WF.CheckBox CheckBox { get { return Control as WF.CheckBox; } }

        public bool Checked
        {
            get { return CheckBox.Checked; }
            set { CheckBox.Checked = value; }
        }

        public override Size2i NaturalSize
        {
            get {
                var pf=CheckBox.PreferredSize;
                return new Size2i(pf.Width, pf.Height);
            }
        }
    }
}
