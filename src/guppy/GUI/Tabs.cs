using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{

    //TODO: Sizing for multiple tabs seems not to work 100%

    public enum TabSide
    {
        Top = 1,
        Left = 2,
        Right = 3,
        Bottom = 4
    }

    public class Tabs:Group
    {
        public Tabs(Group parent)
        {
            GroupLayoutInfo.Sizer = SizerTabs.Instance; //always layout with this specialized layout manager

            Construct(Guppy.Driver.CreateTabs(this));
            if (parent != null)
                parent.Append(this);
        }


        private IDriverTabs DriverTabs {
            get {
                return DriverObject as IDriverTabs;
            }
        }

        public override Sizer Sizer
        {
            set
            {
                /*do nothing, we are not allowed to change layout algorithm for tabs*/
            }
        }

        public TabPage SelectedPage
        {
            get
            {
                return DriverTabs.SelectedPage;
            }
            set
            {
                DriverTabs.SelectedPage = value;
            }
        }

        public TabSide TabSide
        {
            get
            {
                return DriverTabs.TabSide;
            }
            set
            {
                DriverTabs.TabSide = value;
            }
        }

        #region EVENTS

        public event GUIEvent EvChanged;
        protected virtual void OnChanged(EventArgs e) { if (EvChanged != null) EvChanged(this, e); }
        internal void TriggerChanged(EventArgs e) { OnChanged(e); }

        #endregion
    }

    public class TabPage : Group
    {

        public TabPage(Group parent,string caption)
        {
            Construct(Guppy.Driver.CreateTabPage(this,caption));
            if (parent != null)
                parent.Append(this);
        }


        private IDriverTabs DriverTabs
        {
            get
            {
                return DriverObject as IDriverTabs;
            }
        }
    }
}
