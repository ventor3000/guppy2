using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class Frame:Group
    {

        protected IDriverFrame DriverFrame;

        public Frame(Group parent, string caption)
        {
            DriverFrame = Guppy.Driver.CreateFrame(this, caption);
            Construct(DriverFrame);
            if (parent != null)
                parent.Append(this);

            
        }

       /* internal override IDriverGroup DriverGroup
        {
            get { return DriverFrame; }
        }*/
        

        
    }
}
