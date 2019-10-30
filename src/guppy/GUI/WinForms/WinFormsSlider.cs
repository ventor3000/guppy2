using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF=System.Windows.Forms;


namespace Guppy2.GUI.WinForms
{
    public class WinFormsSlider:WinFormsControl,IDriverSlider
    {
        const double maxval = 1000.0; //the internal maximum value of trackbar. Public value range is always 0-1
        int preffered_thickness;

        public WinFormsSlider(GUIObject guiobject,bool vertical)
        {
            Init(new WF.TrackBar(), guiobject);

            var trackbar = TrackBar;

            //stupid getpreffered size doesent work well, only always returns current size so we store it here
            //discounting for the rediculous space taken into account for the tickmarks
            //we currently dont use
            preffered_thickness = trackbar.Height / 2;

            trackbar.Orientation = vertical ? System.Windows.Forms.Orientation.Vertical : System.Windows.Forms.Orientation.Horizontal;
            trackbar.AutoSize = false;
            trackbar.Maximum = (int)maxval;
            trackbar.TickStyle = System.Windows.Forms.TickStyle.None;
            //trackbar.Scroll += delegate { ((Valuator)shellobject).OnChanged(); };

            trackbar.SmallChange = (int)(maxval / 10.0);
            trackbar.LargeChange = (int)(maxval / 4);
        }
        
        private WF.TrackBar TrackBar { get { return Control as WF.TrackBar; } }

        public double Value
        {
            get
            {
                return (double)TrackBar.Value / maxval;
            }
            set
            {
                TrackBar.Value = (int)(Math.Max(Math.Min(value,1.0),0.0) * maxval);
            }
        }

        public override Size2i NaturalSize
        {
            get {
                Size2i res;
                if (TrackBar.Orientation == System.Windows.Forms.Orientation.Horizontal)
                    res = new Size2i(Guppy.DefaultTrackbarLength, preffered_thickness);
                else
                    res = new Size2i(preffered_thickness, Guppy.DefaultTrackbarLength);

                return res;
            }
        }
    }
}
