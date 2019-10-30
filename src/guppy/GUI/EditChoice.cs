using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class EditChoice:Widget
    {
        public EditChoice(Group parent, params object[] items)
        {
            Construct(Guppy.Driver.CreateEditChoice(this));
            if (parent != null)
                parent.Append(this);

            foreach (object i in items)
                Add(i,null);
        }


        private IDriverChoice DriverEditChoice {
            get {
                return DriverObject as IDriverChoice;
            }
        }

        public int Add(object obj, Picture image = null)
        {
            return DriverEditChoice.Add(obj, image);
        }
    }
}
