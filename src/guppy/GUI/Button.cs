using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    //TODO: ImagePosition support
    
    public class Button:Widget
    {
        public string Response = null; //if not set to null, closes the parent dialog when pressed with this string as result

        private Picture image = null;


        public Button(Group parent, string caption) 
        {
            Construct(Guppy.Driver.CreateButton(this, caption));
            if (parent != null)
                parent.Append(this);
        }


        private IDriverButton DriverButton
        {
            get
            {
                return DriverObject as IDriverButton;
            }
        }
       
        public Picture Picture
        {
            get
            {
                return image;
            }
            set
            {
                DriverButton.Picture = value;
                image = value;
            }
        }


        public bool Flat
        {
            get
            {
                return DriverButton.Flat;
            }
            set
            {
                DriverButton.Flat = value;
            }
        }

        public bool CanFocus
        {
            get
            {
                return DriverButton.CanFocus;
            }
            set
            {
                DriverButton.CanFocus = value;
            }
        }


        #region EVENTS

        public event GUIEvent EvClick;
        protected virtual void OnClick(EventArgs e) { 
            if (EvClick != null) EvClick(this,e);

            if (Response != null)
            {
                Window win = Window;
                if (win != null)
                    win.Close(Response);
            }
        }
        internal void TriggerClick(EventArgs e) { OnClick(e); }
        
        #endregion
    }
}
