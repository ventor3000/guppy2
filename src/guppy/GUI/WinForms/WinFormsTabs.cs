using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF = System.Windows.Forms;


namespace Guppy2.GUI.WinForms
{
    public class WinFormsTabs:WinFormsGroupControl,IDriverTabs
    {
        public WinFormsTabs(GUIObject shellobject)
        {
            Init(new WF.TabControl(), shellobject);

            TabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
        }

        void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            Tabs w = TabControl.Tag as Tabs;
            w.TriggerChanged(new EventArgs());
        }


        private WF.TabControl TabControl {get {return Control as WF.TabControl;}}

        public override Size2i NaturalSize
        {
            get { return new Size2i(200,200); /*TODO: compute better natural size from children*/ }
        }


        public Margin BorderDecorMargin
        {
            get
            {
                var dr = TabControl.DisplayRectangle;
                var siz = TabControl.Size;
                return new Margin(dr.Left, dr.Top, siz.Width - dr.Right, siz.Height - dr.Bottom);
               
            }
        }

        public override void Append(IDriverWidget child)
        {
            //appending to tabs does not append a control child in normal order,
            //but appends a tab page to the tab control

            if(child!=null) {
                WinFormsTabPage tp = child as WinFormsTabPage;
                if (tp == null)
                    throw new Exception("Only object of type TabPage can be appended to Tabs");

                TabControl.TabPages.Add(tp.TabPage);
                
            }
        }

        public TabPage SelectedPage
        {
            get
            {
                WF.TabPage tp = TabControl.SelectedTab;
                if (tp == null) 
                    return null;
                return tp.Tag as TabPage;
            }
            set
            {
                foreach(WF.TabPage wfpage in TabControl.TabPages) {
                    if (wfpage.Tag == value)
                    {
                        TabControl.SelectedTab = wfpage;
                        return;
                    }
                }
            }
        }

        public TabSide TabSide {
            get {
                switch(TabControl.Alignment) {
                    case WF.TabAlignment.Left: return TabSide.Left;
                    case WF.TabAlignment.Right: return TabSide.Right;
                    case WF.TabAlignment.Bottom: return TabSide.Bottom;
                    default: 
                    case WF.TabAlignment.Top: return TabSide.Top;
                }
            }
            set
            {
                switch (value)
                {
                    case TabSide.Left: TabControl.Alignment = WF.TabAlignment.Left; break;
                    case TabSide.Right: TabControl.Alignment = WF.TabAlignment.Right; break;
                    case TabSide.Bottom: TabControl.Alignment = WF.TabAlignment.Bottom; break;
                    default:
                    case TabSide.Top: TabControl.Alignment = WF.TabAlignment.Top; break;
                }
            }
        }
    }

    public class WinFormsTabPage : WinFormsGroupControl,IDriverTabPage
    {
        public WinFormsTabPage(GUIObject shellobject, string caption)
        {
            Init(new WF.TabPage(), shellobject);
            TabPage.Text = caption;
        }

        internal WF.TabPage TabPage {get {return Control as WF.TabPage;}}

        public Margin BorderDecorMargin
        {
            get
            {
                return Margin.None; 
            }
        }

       
    }
}
