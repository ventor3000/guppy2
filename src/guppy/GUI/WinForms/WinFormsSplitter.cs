using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF=System.Windows.Forms;


namespace Guppy2.GUI.WinForms
{
    public class WinFormsSplitter:WinFormsGroupControl,IDriverSplitter
    {

        WinFormsSplitterPanel panel1;
        WinFormsSplitterPanel panel2;


        bool block_splitter_changed_event=false;

        public WinFormsSplitter(GUIObject guiobject,bool vertical)
        {
            Init(new WF.SplitContainer(), guiobject);

            
            Splitter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            Vertical = vertical;

            panel1 = new WinFormsSplitterPanel(Widget as Splitter,Splitter.Panel1);
            panel2 = new WinFormsSplitterPanel(Widget as Splitter,Splitter.Panel2);

            Splitter.SplitterMoved += Splitter_SplitterMoved;
        }

        void Splitter_SplitterMoved(object sender, WF.SplitterEventArgs e)
        {
            if (!block_splitter_changed_event)
            {
                (Widget as Group).RefreshChildren();
            }

            int debug = 0;
            
        }

        internal WF.SplitContainer Splitter { get { return Control as WF.SplitContainer; } }


        public Margin BorderDecorMargin
        {
            get
            {
                var split = Splitter;
                int mw = split.Size.Width - split.ClientSize.Width;
                int mh = split.Size.Height - split.ClientSize.Height;

                return new Margin(mw / 2, mh / 2, mw / 2, mh / 2);
            }
        }


        public SplitterPanel Panel1 { get { return panel1.Widget as SplitterPanel; } }
        public SplitterPanel Panel2 { get { return panel2.Widget as SplitterPanel; } }


        public override Rect2i Bounds
        {
            get
            {
                return base.Bounds;
            }
            set
            {
                try
                {
                    block_splitter_changed_event = true;
                    if (Panel1.LayoutResult.MinSize != null)
                    {
                        if (Vertical)
                        {
                            Splitter.Panel1MinSize = Panel1.LayoutResult.MinSize.Width;
                            Splitter.Panel2MinSize = Panel2.LayoutResult.MinSize.Width;
                        }
                        else
                        {
                            Splitter.Panel1MinSize = Panel1.LayoutResult.MinSize.Height;
                            Splitter.Panel2MinSize = Panel2.LayoutResult.MinSize.Height;
                        }



                        /*int flag = 0;
                        if (Vertical ? panel1.Widget.LayoutResult.ExpandX : panel1.Widget.LayoutResult.ExpandY)
                            flag |= 1;
                        if (Vertical ? panel2.Widget.LayoutResult.ExpandX : panel2.Widget.LayoutResult.ExpandY)
                            flag |= 2;

                        switch (flag)
                        {
                            case 0: Splitter.FixedPanel = WF.FixedPanel.None; break;
                            case 1: Splitter.FixedPanel = WF.FixedPanel.Panel1; break;
                            case 2: Splitter.FixedPanel = WF.FixedPanel.Panel2; break;
                            case 3: Splitter.FixedPanel = WF.FixedPanel.None; break;
                        }*/

                    }

                    base.Bounds = value;
                }
                finally
                {
                    block_splitter_changed_event = false;
                }
            }
        }


        public bool Vertical
        {
            get
            {
                return Splitter.Orientation == WF.Orientation.Vertical;
            }
            set
            {
                Splitter.Orientation = value ? WF.Orientation.Vertical : WF.Orientation.Horizontal;
            }
        }

        public int SplitterWidth
        {
            get { return Splitter.SplitterWidth; }
            set { Splitter.SplitterWidth = Math.Min(1, value); } //.net splitters do not allow for <1 splitterwidth
        }

        public int SplitterPosition
        {
            get
            {
                return Splitter.SplitterDistance;
            }

            set
            {
                Splitter.SplitterDistance = value;
            }
        }

      
      
    }
}
