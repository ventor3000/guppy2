using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF = System.Windows.Forms;

namespace Guppy2.GUI.WinForms
{
    public class WinFormsProgressBar:WinFormsControl,IDriverProgressBar
    {


        public WinFormsProgressBar(GUIObject shellobject)
        {
            Init(new WF.ProgressBar(), shellobject);

            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = 100;

            //progbar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            ProgressBar.MarqueeAnimationSpeed = 1000;
        }

        private WF.ProgressBar ProgressBar
        {
            get { return Control as WF.ProgressBar; }
        }

        public double Value
        {
            get
            {
                return (double)ProgressBar.Value / 100.0;
            }
            set
            {
                ProgressBar.Value = (int)(value * 100.0);
            }
        }


        public override Size2i NaturalSize
        {
            get { return new Size2i(120,25); }
        }
    }
}
