using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class ProgressBar:Widget
    {
        private double minval = 0.0;
        private double maxval = 1.0;

        public ProgressBar(Group parent)
        {
            Construct(Guppy.Driver.CreateProgressBar(this));
            if (parent != null)
                parent.Append(this);
        }


        private IDriverProgressBar DriverProgress {get {return DriverObject as IDriverProgressBar;}}

        virtual public double Value
        {
            //internally, driver wants value 0.0-1.0 so we recompute them
            get
            {
                return DriverProgress.Value * (maxval - minval) + minval;
            }
            set
            {
                double delta = (maxval - minval); //protect from invalid settings
                if (delta <= 1e-6)
                    DriverProgress.Value = 0.0;
                else
                {
                    value = (value - minval) / delta;   //recompute value for drivers 0.0-1.0
                    if (value < 0.0) value = 0.0;
                    if (value > 1.0) value = 1.0;
                    DriverProgress.Value = value;
                }
            }
        }

        virtual public double Max
        {
            get { return maxval; }
            set { maxval = value; }
        }

        virtual public double Min
        {
            get { return minval; }
            set { minval = value; }
        }

        
    }
}
