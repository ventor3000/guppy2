using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class Label : Widget
    {
        public Label(Group parent, string caption)
        {
            Construct(Guppy.Driver.CreateLabel(this, caption));
            if (parent != null)
                parent.Append(this);
        }


         private IDriverLabel DriverLabel
        {
            get
            {
                return DriverObject as IDriverLabel;
            }
        }

         public virtual string Caption
         {
             get
             {
                 return DriverLabel.Caption;
             }
             set
             {
                 DriverLabel.Caption = value;
             }
         }
    }
}
