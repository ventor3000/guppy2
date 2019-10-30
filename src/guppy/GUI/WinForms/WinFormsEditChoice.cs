using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF = System.Windows.Forms;

namespace Guppy2.GUI.WinForms
{
    public class WinFormsEditChoice:WinFormsChoice,IDriverEditChoice
    {
        public WinFormsEditChoice(GUIObject guiobj):base(guiobj)
        {
            ComboBox.DropDownStyle = WF.ComboBoxStyle.DropDown;
        }

    }
}
