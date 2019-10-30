using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class Edit:Widget
    {
        public Edit(Group parent) 
        {
            Construct(Guppy.Driver.CreateEdit(this));
            if (parent != null)
                parent.Append(this);
        }

        public string Text {
            get {return ((IDriverEdit)DriverControl).Text;}
            set { ((IDriverEdit)DriverControl).Text = value ?? ""; }
        }

        #region EVENTS
        
       
       

        public event GUIEventKey EvKeyDown;
        protected virtual void OnKeyDown(KeyEventArgs e) { if (EvKeyDown != null) EvKeyDown(this,e); }
        internal void TriggerKeyDown(KeyEventArgs e) { OnKeyDown(e); }


        #endregion
    }
}
