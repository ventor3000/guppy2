using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class Fill:Widget
    {
        public Fill(Group parent)
        {
            Construct(null);
            if (parent != null)
                parent.Append(this);
        }

    }
}
