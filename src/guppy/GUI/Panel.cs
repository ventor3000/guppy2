using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class Panel:Group
    {
        protected IDriverPanel DriverPanel;

        public Panel(Group parent) 
        {
            DriverPanel = Guppy.Driver.CreatePanel(this);
            Construct(DriverPanel);
            if (parent != null)
                parent.Append(this);
        }
    }
}
