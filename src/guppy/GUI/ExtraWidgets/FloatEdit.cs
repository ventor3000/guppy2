using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;

namespace Guppy2.GUI.ExtraWidgets
{
    public class FloatEdit:Edit
    {

        private string entrytext;
        private double currentvalue = 0.0;

        public FloatEdit(Group parent)
            : base(parent)
        {
            Value = 0;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            char ch = e.KeyData.Char;
            if (!char.IsDigit(ch) && ch != '.' && ch != ',' && ch != '-' && ch >= 32)
                e.Block = true;

            base.OnKeyDown(e);
        }

        protected override void OnEnter(EventArgs e)
        {
            entrytext = Text;
            base.OnEnter(e);
        }

        protected override void OnLeave(BlockableEventArgs e)
        {
            double d;
            if (double.TryParse(Text, NumberStyles.Float,Thread.CurrentThread.CurrentUICulture, out d))
                currentvalue = d;
            else
            {
                Text = entrytext;
                e.Block = true;
            }

            base.OnLeave(e);
        }

        public double Value
        {
            get
            {
                return currentvalue;
            }
            set
            {
                currentvalue = value;
                UpdateText();
            }
        }

        private void UpdateText()
        {
            Text = currentvalue.ToString(Thread.CurrentThread.CurrentUICulture);
        }
        

        
    }
}
