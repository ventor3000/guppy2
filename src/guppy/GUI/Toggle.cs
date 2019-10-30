using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class Toggle:Widget
    {
        public Toggle(Group parent, string caption) 
        {
            Construct(Guppy.Driver.CreateToggle(this, caption));
            if (parent != null)
                parent.Append(this);
        }
    }
}
