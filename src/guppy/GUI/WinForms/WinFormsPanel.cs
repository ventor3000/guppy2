using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF=System.Windows.Forms;

namespace Guppy2.GUI.WinForms
{
    public class WinFormsPanel : WinFormsGroupControl,IDriverPanel
    {
        

        public WinFormsPanel(GUIObject guiobject)
        {
            Init(new WF.Panel(), guiobject);
        }


      

        public Margin BorderDecorMargin
        {
            get
            {
                return Margin.None; //default is no border
            }
        }

        internal WF.Panel Panel { get { return Control as WF.Panel; } }
    }
}
