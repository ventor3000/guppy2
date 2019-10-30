using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class Slider:Widget
    {

        protected IDriverSlider DriverSlider;

        private double minval = 0.0;    //driver always wants values between 0.0-1.0, we recompute thoose if changed...
        private double maxval = 1.0;

        public Slider(Group parent,bool vertical)
        {
            DriverSlider = Guppy.Driver.CreateSlider(this,vertical);
            Construct(DriverSlider);
            if (parent != null)
                parent.Append(this);
        }

        public double Value
        {
            get
            {
                return DriverSlider.Value * (maxval - minval) + minval;
            }
            set
            {
                double delta = (maxval - minval); //protect from invalid settings (max<min)
                if (delta <= 1e-6)
                    DriverSlider.Value = 0.0;
                else
                {
                    value = (value - minval) / delta;   //recompute value for drivers 0.0-1.0
                    DriverSlider.Value = value;
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
