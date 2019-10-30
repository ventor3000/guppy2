using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class Separator : Widget
    {
        public Separator(Group parent, bool vertical)
        {
            Construct(Guppy.Driver.CreateSeparator(this, vertical));
            if (parent != null)
                parent.Append(this);
            if (vertical)
                ExpandY = true;
            else
                ExpandX = true;
        }
    }
}
