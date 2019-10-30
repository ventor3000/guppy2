using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class Choice:Widget
    {

        public Choice(Group parent, params object[] items)
        {
            Construct(Guppy.Driver.CreateChoice(this));
            if (parent != null)
                parent.Append(this);

            foreach (object i in items)
                Add(i,null);
        }


        private IDriverChoice DriverChoice {
            get {
                return DriverObject as IDriverChoice;
            }
        }

        public int Add(object obj, Picture image = null)
        {
            return DriverChoice.Add(obj, image);
        }

    }
}
