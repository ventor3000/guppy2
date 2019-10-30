using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class SplitterPanel:Group
    {
        protected IDriverSplitterPanel DriverPanel;

        internal SplitterPanel(Splitter parent,IDriverSplitterPanel drvpanel)
        {
            //internal because user should never craete this directly, only access through Splitter control
            DriverPanel = drvpanel;
            Construct(DriverPanel);
            if (parent != null)
                parent.Append(this);
        }
    }
}
