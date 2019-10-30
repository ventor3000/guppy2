using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF=System.Windows.Forms;


namespace Guppy2.GUI.WinForms
{
    public class WinFormsSplitterPanel:WinFormsGroupControl,IDriverSplitterPanel
    {
        WF.Panel panel;

        public WinFormsSplitterPanel(Splitter parent,WF.Panel panel)
        {
            this.panel = panel;
            var sp = new SplitterPanel(parent, this);
            Init(panel, sp);
        }

        

        public Margin BorderDecorMargin
        {
            get
            {

                var cs = panel.ClientSize;
                var s = panel.Size;
                int mw = (s.Width - cs.Width) / 2;
                int mh = (s.Height - cs.Height) / 2;
                return new Margin(mw,mh,mw,mh);
            }
        }

        public override Rect2i Bounds
        {
            get
            {
                return base.Bounds;
                //int debug = 0;
            }
            set
            {
                //base.Bounds = value;
                int debug = 0;
            }
        }
    }
}
