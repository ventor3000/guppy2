using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class Splitter:Group
    {

        protected IDriverSplitter DriverSplitter;

        public Splitter(Group parent,bool vertical)
        {
            
            GroupLayoutInfo.Sizer = SizerSplitter.Instance; //splitter is hardcoded for thios sizer

            DriverSplitter = Guppy.Driver.CreateSplitter(this,vertical);
            Construct(DriverSplitter);
            if (parent != null)
                parent.Append(this);
        }

        /*public override void Append(Widget ctrl)
        {
            //do nothing => cant append to splitters
        }*/


        public override Sizer Sizer
        {
            set
            {
                /*do nothing, we are not allowed to change layout algorithm for splitter, its hardwired to SizerSplitter*/
            }
        }


        public override bool Vertical
        {
            get
            {
                return base.Vertical;
            }
            set
            {
                DriverSplitter.Vertical = value;
                base.Vertical = value;
            }
        }

        public int SplitterWidth
        {
            get { return DriverSplitter.SplitterWidth; }
            set { DriverSplitter.SplitterWidth = value; }
        }


        public int SplitterPosition
        {
            get { return DriverSplitter.SplitterPosition; }
            set { DriverSplitter.SplitterPosition = value; }
        }


        public SplitterPanel Panel1 { get { return DriverSplitter.Panel1; } }
        public SplitterPanel Panel2 { get { return DriverSplitter.Panel2; } }

        
    }
}
